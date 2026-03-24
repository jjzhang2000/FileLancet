namespace FileLancet.Core.Models;

public class ParseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public FileNode? RootNode { get; set; }
    public FileDetails? Details { get; set; }
    public TimeSpan ParseTime { get; set; }
}
