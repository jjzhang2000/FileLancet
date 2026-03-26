namespace FileLancet.Core.Models;

/// <summary>
/// 解析结果
/// </summary>
public class ParseResult
{
    /// <summary>是否成功</summary>
    public bool Success { get; set; }

    /// <summary>错误信息</summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>文件根节点</summary>
    public FileNode? RootNode { get; set; }

    /// <summary>文件详细信息</summary>
    public FileDetails? Details { get; set; }

    /// <summary>原始文件路径</summary>
    public string SourcePath { get; set; } = string.Empty;
}
