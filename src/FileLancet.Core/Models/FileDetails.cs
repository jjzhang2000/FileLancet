namespace FileLancet.Core.Models;

/// <summary>
/// EPUB 文件详细信息
/// </summary>
public class FileDetails
{
    // 物理属性
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime CreatedTime { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;

    // EPUB 元数据
    public string Title { get; set; } = string.Empty;
    public List<string> Authors { get; set; } = new();
    public string Publisher { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? PublicationDate { get; set; }
    public string EpubVersion { get; set; } = string.Empty;

    // 统计信息
    public int ChapterCount { get; set; }
    public int ImageCount { get; set; }
    public int StylesheetCount { get; set; }
    public int TotalFileCount { get; set; }
}
