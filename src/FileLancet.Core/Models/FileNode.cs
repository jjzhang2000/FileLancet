namespace FileLancet.Core.Models;

/// <summary>
/// 文件节点
/// </summary>
public class FileNode
{
    /// <summary>节点名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>节点路径（相对于容器）</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>节点类型</summary>
    public NodeType Type { get; set; }

    /// <summary>子节点列表</summary>
    public List<FileNode> Children { get; set; } = new();

    /// <summary>节点描述（用于状态栏提示）</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>文件大小（字节）</summary>
    public long Size { get; set; }

    /// <summary>MIME 类型</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>是否已加载内容</summary>
    public bool IsContentLoaded { get; set; }

    /// <summary>父节点引用</summary>
    public FileNode? Parent { get; set; }

    /// <summary>
    /// 获取完整路径
    /// </summary>
    public string GetFullPath()
    {
        if (Parent == null)
            return Path;
        
        var parentPath = Parent.GetFullPath();
        if (string.IsNullOrEmpty(parentPath))
            return Path;
        
        return System.IO.Path.Combine(parentPath, Path);
    }

    /// <summary>
    /// 添加子节点
    /// </summary>
    public void AddChild(FileNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }
}
