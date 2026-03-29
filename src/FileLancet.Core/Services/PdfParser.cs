using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;
using UglyToad.PdfPig;

namespace FileLancet.Core.Services;

/// <summary>
/// PDF 文件解析器
/// </summary>
public class PdfParser : IFileLancetParser
{
    /// <inheritdoc />
    public bool CanParse(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;

        return Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public ParseResult Parse(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);

            // 提取文档信息
            var details = ExtractPdfDetails(document, filePath);

            // 构建文件树
            var rootNode = BuildPdfTree(document, filePath);

            return new ParseResult
            {
                Success = true,
                RootNode = rootNode,
                Details = details,
                SourcePath = filePath
            };
        }
        catch (Exception ex)
        {
            return new ParseResult
            {
                Success = false,
                ErrorMessage = $"PDF 解析失败: {ex.Message}",
                SourcePath = filePath
            };
        }
    }

    /// <inheritdoc />
    public async Task<ParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Parse(filePath);
        }, cancellationToken);
    }

    /// <summary>
    /// 提取 PDF 详细信息
    /// </summary>
    private PdfDetails ExtractPdfDetails(PdfDocument document, string filePath)
    {
        var info = document.Information;
        var fileInfo = new FileInfo(filePath);

        var details = new PdfDetails
        {
            Title = info.Title ?? Path.GetFileNameWithoutExtension(filePath),
            Authors = string.IsNullOrEmpty(info.Author)
                ? new List<string>()
                : new List<string> { info.Author },
            Subject = info.Subject,
            Keywords = info.Keywords,
            Creator = info.Creator,
            Producer = info.Producer,
            PdfVersion = document.Version.ToString(),
            PageCount = document.NumberOfPages,
            IsEncrypted = document.IsEncrypted,
            FilePath = filePath,
            FileSize = fileInfo.Length,
            LastModified = fileInfo.LastWriteTime,
            CreatedTime = fileInfo.CreationTime,
            FileExtension = ".pdf",
            MimeType = "application/pdf"
        };

        // 提取页面信息
        for (int i = 1; i <= document.NumberOfPages; i++)
        {
            try
            {
                var page = document.GetPage(i);
                details.Pages.Add(new PdfPageInfo
                {
                    PageNumber = i,
                    Width = page.Width,
                    Height = page.Height,
                    Rotation = (int)page.Rotation.Value
                });
            }
            catch
            {
                // 如果某页无法读取，添加基本信息
                details.Pages.Add(new PdfPageInfo
                {
                    PageNumber = i,
                    Width = 0,
                    Height = 0,
                    Rotation = 0
                });
            }
        }

        return details;
    }

    /// <summary>
    /// 构建 PDF 文件树
    /// </summary>
    private FileNode BuildPdfTree(PdfDocument document, string filePath)
    {
        var root = new FileNode
        {
            Name = Path.GetFileName(filePath),
            Path = filePath,
            Type = NodeType.PdfDocument,
            Description = $"PDF 文档 - {document.NumberOfPages} 页"
        };

        // 添加页面节点
        for (int i = 1; i <= document.NumberOfPages; i++)
        {
            try
            {
                var page = document.GetPage(i);
                root.Children.Add(new FileNode
                {
                    Name = $"Page {i}",
                    Path = $"{filePath}#page={i}",
                    Type = NodeType.PdfPage,
                    Description = $"第 {i} 页 - {page.Width:F0}x{page.Height:F0}"
                });
            }
            catch
            {
                // 如果某页无法读取，仍添加节点
                root.Children.Add(new FileNode
                {
                    Name = $"Page {i}",
                    Path = $"{filePath}#page={i}",
                    Type = NodeType.PdfPage,
                    Description = $"第 {i} 页"
                });
            }
        }

        // 添加大纲/书签节点（如果有）
        // TODO: 书签功能待实现

        return root;
    }
}
