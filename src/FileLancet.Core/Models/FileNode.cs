using System.Collections.ObjectModel;

namespace FileLancet.Core.Models;

public class FileNode
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public NodeType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime? LastModified { get; set; }
    public ObservableCollection<FileNode> Children { get; set; } = new();
    public FileNode? Parent { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsSelected { get; set; }
    public string? MimeType { get; set; }
    public bool IsLazyLoaded { get; set; }

    public bool HasChildren => Children.Count > 0;

    public string FullPath => Parent != null 
        ? System.IO.Path.Combine(Parent.FullPath, Path) 
        : Path;
}
