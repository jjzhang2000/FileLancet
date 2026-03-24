using System.IO.Compression;
using System.Xml.Linq;
using FileLancet.Core.Models;

namespace FileLancet.Core.Services;

public class EpubParser : IFileLancetParser
{
    public string[] SupportedExtensions => new[] { ".epub" };
    public string ParserName => "EPUB Parser";

    public bool CanParse(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".epub")
            return false;

        try
        {
            using var archive = ZipFile.OpenRead(filePath);
            return archive.GetEntry("META-INF/container.xml") != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            using var archive = ZipFile.OpenRead(filePath);
            
            var containerEntry = archive.GetEntry("META-INF/container.xml")
                ?? throw new InvalidOperationException("META-INF/container.xml not found");

            string opfPath;
            using (var stream = containerEntry.Open())
            {
                var containerDoc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
                var rootfile = containerDoc.Root?.Element("rootfiles")?.Element("rootfile")
                    ?? throw new InvalidOperationException("Invalid container.xml structure");
                opfPath = rootfile.Attribute("full-path")?.Value 
                    ?? throw new InvalidOperationException("OPF path not found in container.xml");
            }

            var opfEntry = archive.GetEntry(opfPath)
                ?? throw new InvalidOperationException($"OPF file not found: {opfPath}");

            BookMetadata? metadata = null;
            using (var stream = opfEntry.Open())
            {
                metadata = await ParseOpfAsync(stream, cancellationToken);
            }

            var rootNode = new FileNode
            {
                Name = System.IO.Path.GetFileName(filePath),
                Path = filePath,
                Type = NodeType.Root,
                Description = "EPUB电子书文件"
            };

            await BuildFileTreeAsync(archive, rootNode, opfPath, cancellationToken);

            var fileInfo = new FileInfo(filePath);
            var details = new FileDetails
            {
                Name = fileInfo.Name,
                Path = fileInfo.FullName,
                Size = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime,
                Metadata = metadata
            };

            return new ParseResult
            {
                Success = true,
                RootNode = rootNode,
                Details = details,
                ParseTime = DateTime.UtcNow - startTime
            };
        }
        catch (Exception ex)
        {
            return new ParseResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ParseTime = DateTime.UtcNow - startTime
            };
        }
    }

    private async Task<BookMetadata> ParseOpfAsync(Stream stream, CancellationToken cancellationToken)
    {
        var doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var metadata = new BookMetadata();
        
        var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;
        var metadataElement = doc.Root?.Element(ns + "metadata");
        
        if (metadataElement != null)
        {
            metadata.Title = metadataElement.Element(ns + "title")?.Value;
            metadata.Language = metadataElement.Element(ns + "language")?.Value;
            metadata.Description = metadataElement.Element(ns + "description")?.Value;
            
            var creator = metadataElement.Element(ns + "creator");
            if (creator != null)
                metadata.Author = creator.Value;
            
            var publisher = metadataElement.Element(ns + "publisher");
            if (publisher != null)
                metadata.Publisher = publisher.Value;
            
            var identifier = metadataElement.Element(ns + "identifier");
            if (identifier != null)
                metadata.Isbn = identifier.Value;
            
            var date = metadataElement.Element(ns + "date");
            if (date != null && DateTime.TryParse(date.Value, out var pubDate))
                metadata.PublicationDate = pubDate;

            metadata.Subjects = metadataElement.Elements(ns + "subject")
                .Select(s => s.Value)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        var version = doc.Root?.Attribute("version")?.Value;
        if (version != null)
            metadata.EpubVersion = version;

        return metadata;
    }

    private async Task BuildFileTreeAsync(ZipArchive archive, FileNode rootNode, string opfPath, CancellationToken cancellationToken)
    {
        var basePath = System.IO.Path.GetDirectoryName(opfPath)?.Replace('\\', '/') ?? "";
        
        var folders = new Dictionary<string, FileNode>();
        folders[""] = rootNode;

        foreach (var entry in archive.Entries.OrderBy(e => e.FullName))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entryPath = entry.FullName;
            var parts = entryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            var currentPath = "";
            FileNode? parentNode = rootNode;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var isLast = i == parts.Length - 1;
                
                currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                if (isLast)
                {
                    var fileNode = new FileNode
                    {
                        Name = part,
                        Path = entryPath,
                        Type = GetNodeType(part),
                        Size = entry.Length,
                        LastModified = entry.LastWriteTime.DateTime,
                        Parent = parentNode,
                        Description = GetFileDescription(part),
                        MimeType = GetMimeType(part)
                    };
                    parentNode?.Children.Add(fileNode);
                }
                else
                {
                    if (!folders.ContainsKey(currentPath))
                    {
                        var folderNode = new FileNode
                        {
                            Name = part,
                            Path = currentPath,
                            Type = NodeType.Folder,
                            Parent = parentNode,
                            Description = $"文件夹: {part}"
                        };
                        folders[currentPath] = folderNode;
                        parentNode?.Children.Add(folderNode);
                    }
                    parentNode = folders[currentPath];
                }
            }
        }

        await Task.CompletedTask;
    }

    private static NodeType GetNodeType(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".html" or ".xhtml" or ".htm" => NodeType.Html,
            ".css" => NodeType.Css,
            ".xml" or ".opf" or ".ncx" => NodeType.Configuration,
            ".txt" or ".md" => NodeType.Text,
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".svg" or ".webp" => NodeType.Image,
            ".ttf" or ".otf" or ".woff" or ".woff2" => NodeType.Font,
            ".mp3" or ".ogg" or ".wav" => NodeType.Audio,
            ".mp4" or ".webm" => NodeType.Video,
            _ => NodeType.Binary
        };
    }

    private static string GetMimeType(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".html" or ".htm" => "text/html",
            ".xhtml" => "application/xhtml+xml",
            ".css" => "text/css",
            ".xml" => "application/xml",
            ".opf" => "application/oebps-package+xml",
            ".ncx" => "application/x-dtbncx+xml",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".ttf" => "font/ttf",
            ".otf" => "font/otf",
            _ => "application/octet-stream"
        };
    }

    private static string GetFileDescription(string fileName)
    {
        var lowerName = fileName.ToLowerInvariant();
        
        if (lowerName == "container.xml")
            return "EPUB容器配置文件，定义OPF文件位置";
        if (lowerName.EndsWith(".opf"))
            return "OPF包文档，包含书籍元数据和资源清单";
        if (lowerName.EndsWith(".ncx"))
            return "NCX目录文件，定义书籍导航结构";
        if (lowerName.EndsWith(".xhtml") || lowerName.EndsWith(".html"))
            return "HTML内容文件";
        if (lowerName.EndsWith(".css"))
            return "CSS样式表";
        
        return $"文件: {fileName}";
    }
}
