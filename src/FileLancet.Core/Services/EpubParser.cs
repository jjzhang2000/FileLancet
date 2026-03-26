using System.IO.Compression;
using System.Xml.Linq;
using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;
using FileLancet.Core.Utilities;

namespace FileLancet.Core.Services;

/// <summary>
/// EPUB 文件解析器
/// </summary>
public class EpubParser : IFileLancetParser
{
    public bool CanParse(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".epub")
            return false;

        try
        {
            using var stream = File.OpenRead(filePath);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            // 使用 XmlParserHelper 获取条目（支持多种路径格式）
            var containerEntry = XmlParserHelper.GetZipEntry(archive, "META-INF/container.xml");
            return containerEntry != null;
        }
        catch
        {
            return false;
        }
    }

    public ParseResult Parse(string filePath)
    {
        return ParseAsync(filePath).GetAwaiter().GetResult();
    }

    public async Task<ParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!CanParse(filePath))
            {
                return new ParseResult
                {
                    Success = false,
                    ErrorMessage = "不是有效的 EPUB 文件",
                    SourcePath = filePath
                };
            }

            var fileInfo = new FileInfo(filePath);
            var details = new FileDetails
            {
                FilePath = filePath,
                FileSize = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime
            };

            using var stream = File.OpenRead(filePath);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var rootNode = new FileNode
            {
                Name = Path.GetFileName(filePath),
                Path = "",
                Type = NodeType.Root,
                Description = "EPUB 电子书根节点"
            };

            // 读取 container.xml 获取 OPF 路径
            var containerEntry = XmlParserHelper.GetZipEntry(archive, "META-INF/container.xml");
            if (containerEntry == null)
            {
                return new ParseResult
                {
                    Success = false,
                    ErrorMessage = "未找到 META-INF/container.xml",
                    SourcePath = filePath
                };
            }

            string opfPath;
            try
            {
                var containerDoc = await XmlParserHelper.LoadFromZipEntryAsync(containerEntry, cancellationToken);
                var ns = containerDoc.Root?.GetDefaultNamespace() ?? XNamespace.None;
                var rootfile = containerDoc.Root?.Element(ns + "rootfiles")?.Element(ns + "rootfile");
                opfPath = rootfile?.Attribute("full-path")?.Value ?? "";
            }
            catch (Exception ex)
            {
                return new ParseResult
                {
                    Success = false,
                    ErrorMessage = $"解析 container.xml 失败: {ex.Message}",
                    SourcePath = filePath
                };
            }

            if (string.IsNullOrEmpty(opfPath))
            {
                return new ParseResult
                {
                    Success = false,
                    ErrorMessage = "无法获取 OPF 文件路径",
                    SourcePath = filePath
                };
            }

            // 解析 OPF 文件
            var opfEntry = XmlParserHelper.GetZipEntry(archive, opfPath);
            
            if (opfEntry == null)
            {
                // 尝试在根目录下查找 .opf 文件
                opfEntry = archive.Entries.FirstOrDefault(e => 
                    e.FullName.EndsWith(".opf", StringComparison.OrdinalIgnoreCase));
                
                if (opfEntry == null)
                {
                    return new ParseResult
                    {
                        Success = false,
                        ErrorMessage = $"未找到 OPF 文件: {opfPath}",
                        SourcePath = filePath
                    };
                }
            }

            XDocument opfDoc;
            try
            {
                opfDoc = await XmlParserHelper.LoadFromZipEntryAsync(opfEntry, cancellationToken);
            }
            catch (Exception ex)
            {
                return new ParseResult
                {
                    Success = false,
                    ErrorMessage = $"解析 OPF 文件失败: {ex.Message}",
                    SourcePath = filePath
                };
            }

            // 提取元数据
            ExtractMetadata(opfDoc, details);

            // 构建文件树
            BuildFileTree(archive, rootNode, opfPath, details);

            details.TotalFileCount = archive.Entries.Count;

            return new ParseResult
            {
                Success = true,
                RootNode = rootNode,
                Details = details,
                SourcePath = filePath
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new ParseResult
            {
                Success = false,
                ErrorMessage = $"解析失败: {ex.Message}",
                SourcePath = filePath
            };
        }
    }

    private void ExtractMetadata(XDocument opfDoc, FileDetails details)
    {
        // 使用 XmlParserHelper 获取 metadata 元素
        var metadata = XmlParserHelper.GetFirstElementByLocalName(opfDoc, "metadata");
        if (metadata == null) return;

        // 使用本地名称（忽略命名空间）来查找元素
        // 标题
        var title = XmlParserHelper.GetElementValueByLocalName(metadata, "title");
        if (!string.IsNullOrEmpty(title))
            details.Title = title;

        // 作者
        var creators = XmlParserHelper.GetElementsByLocalName(metadata, "creator");
        foreach (var creator in creators)
        {
            var name = creator.Value;
            if (!string.IsNullOrEmpty(name))
                details.Authors.Add(name);
        }

        // 出版社
        var publisher = XmlParserHelper.GetElementValueByLocalName(metadata, "publisher");
        if (!string.IsNullOrEmpty(publisher))
            details.Publisher = publisher;

        // 语言
        var language = XmlParserHelper.GetElementValueByLocalName(metadata, "language");
        if (!string.IsNullOrEmpty(language))
            details.Language = language;

        // ISBN
        var identifier = XmlParserHelper.GetElementValueByLocalName(metadata, "identifier");
        if (!string.IsNullOrEmpty(identifier))
            details.Isbn = identifier;

        // 描述
        var description = XmlParserHelper.GetElementValueByLocalName(metadata, "description");
        if (!string.IsNullOrEmpty(description))
            details.Description = description;

        // EPUB 版本
        var version = opfDoc.Root?.Attribute("version")?.Value;
        if (!string.IsNullOrEmpty(version))
            details.EpubVersion = version;
    }

    private void BuildFileTree(ZipArchive archive, FileNode rootNode, string opfPath, FileDetails details)
    {
        var opfDir = Path.GetDirectoryName(opfPath)?.Replace("\\", "/") ?? "";

        // 创建文件夹结构
        var folders = new Dictionary<string, FileNode>();
        folders[""] = rootNode;

        foreach (var entry in archive.Entries)
        {
            var path = entry.FullName;
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            FileNode current = rootNode;
            var currentPath = "";

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                if (i == parts.Length - 1)
                {
                    // 文件节点
                    var nodeType = GetNodeType(part);
                    var fileNode = new FileNode
                    {
                        Name = part,
                        Path = currentPath,
                        Type = nodeType,
                        Size = entry.Length,
                        Description = GetNodeDescription(nodeType)
                    };
                    current.AddChild(fileNode);

                    // 统计
                    if (nodeType == NodeType.Html)
                        details.ChapterCount++;
                    else if (nodeType == NodeType.Image)
                        details.ImageCount++;
                    else if (nodeType == NodeType.Css)
                        details.StylesheetCount++;
                }
                else
                {
                    // 文件夹节点
                    if (!folders.ContainsKey(currentPath))
                    {
                        var folderNode = new FileNode
                        {
                            Name = part,
                            Path = currentPath,
                            Type = NodeType.Folder,
                            Description = "文件夹"
                        };
                        current.AddChild(folderNode);
                        folders[currentPath] = folderNode;
                    }
                    current = folders[currentPath];
                }
            }
        }
    }

    private NodeType GetNodeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".html" or ".xhtml" or ".htm" => NodeType.Html,
            ".css" => NodeType.Css,
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" => NodeType.Image,
            ".ttf" or ".otf" or ".woff" or ".woff2" => NodeType.Font,
            ".mp3" or ".wav" or ".ogg" or ".aac" => NodeType.Audio,
            ".mp4" or ".webm" or ".ogv" => NodeType.Video,
            ".js" => NodeType.Script,
            ".opf" => NodeType.Opf,
            ".ncx" => NodeType.Ncx,
            ".xml" => fileName.Contains("container") ? NodeType.Container : NodeType.Other,
            _ => NodeType.Other
        };
    }

    private string GetNodeDescription(NodeType type)
    {
        return type switch
        {
            NodeType.Root => "EPUB 电子书根节点",
            NodeType.Folder => "文件夹",
            NodeType.Container => "EPUB 容器配置文件",
            NodeType.Opf => "OPF 包文件（元数据和资源清单）",
            NodeType.Ncx => "NCX 目录文件",
            NodeType.Nav => "NAV 导航文件",
            NodeType.Html => "HTML/XHTML 内容文件",
            NodeType.Css => "CSS 样式表",
            NodeType.Image => "图片资源",
            NodeType.Font => "字体文件",
            NodeType.Audio => "音频文件",
            NodeType.Video => "视频文件",
            NodeType.Script => "JavaScript 脚本",
            _ => "其他文件"
        };
    }
}
