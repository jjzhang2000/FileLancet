namespace FileLancet.Core.Models;

public class FileDetails
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? MimeType { get; set; }
    public DateTime? LastModified { get; set; }
    public double? CompressionRatio { get; set; }

    public BookMetadata? Metadata { get; set; }
}

public class BookMetadata
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public string? Language { get; set; }
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public DateTime? PublicationDate { get; set; }
    public string? EpubVersion { get; set; }
    public List<string> Contributors { get; set; } = new();
    public List<string> Subjects { get; set; } = new();
}
