using System.Xml.Linq;
using FileLancet.Core.Factories;
using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;

namespace FileLancet.Core.Services;

/// <summary>
/// 解析器抽象基类
/// </summary>
public abstract class BaseParser : IFileLancetParser
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    protected ILogger<BaseParser>? Logger { get; set; }

    /// <summary>
    /// 内容加载器
    /// </summary>
    protected IContentLoader? ContentLoader { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    protected BaseParser(ILogger<BaseParser>? logger = null)
    {
        Logger = logger;
    }

    /// <inheritdoc />
    public abstract bool CanParse(string filePath);

    /// <inheritdoc />
    public abstract ParseResult Parse(string filePath);

    /// <inheritdoc />
    public virtual Task<ParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Parse(filePath), cancellationToken);
    }

    /// <summary>
    /// 通用异常处理
    /// </summary>
    protected ParseResult HandleException(Exception ex, string filePath)
    {
        Logger?.LogError(ex, "解析文件失败: {FilePath}", filePath);
        return new ParseResult
        {
            Success = false,
            ErrorMessage = $"解析失败: {ex.Message}",
            SourcePath = filePath
        };
    }

    /// <summary>
    /// 创建文件节点
    /// </summary>
    protected FileNode CreateNode(string name, string path, NodeType type)
    {
        return new FileNode
        {
            Name = name,
            Path = path,
            Type = type,
            Description = GetNodeDescription(type)
        };
    }

    /// <summary>
    /// 获取节点描述
    /// </summary>
    protected abstract string GetNodeDescription(NodeType type);

    /// <summary>
    /// 创建文件详情
    /// </summary>
    protected FileDetails CreateFileDetails(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        return new FileDetails
        {
            Title = Path.GetFileNameWithoutExtension(filePath),
            FilePath = filePath,
            FileSize = fileInfo.Exists ? fileInfo.Length : 0,
            LastModified = fileInfo.Exists ? fileInfo.LastWriteTime : DateTime.MinValue
        };
    }

    /// <summary>
    /// 获取文件 MIME 类型
    /// </summary>
    protected string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".epub" => "application/epub+zip",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// 根据文件扩展名获取节点类型
    /// </summary>
    protected NodeType GetNodeTypeFromExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".html" or ".htm" or ".xhtml" => NodeType.Html,
            ".css" => NodeType.Css,
            ".js" => NodeType.Script,
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" => NodeType.Image,
            ".ttf" or ".otf" or ".woff" or ".woff2" => NodeType.Font,
            ".mp3" or ".ogg" or ".wav" or ".m4a" => NodeType.Audio,
            ".mp4" or ".webm" or ".ogv" => NodeType.Video,
            ".ncx" => NodeType.Ncx,
            ".opf" => NodeType.Opf,
            _ => NodeType.Other
        };
    }
}

/// <summary>
/// 简易日志接口（用于基础解析器）
/// </summary>
public interface ILogger<T>
{
    void LogError(Exception exception, string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogInformation(string message, params object[] args);
}

/// <summary>
/// 空日志实现（默认）
/// </summary>
public class NullLogger<T> : ILogger<T>
{
    public static NullLogger<T> Instance { get; } = new NullLogger<T>();

    public void LogError(Exception exception, string message, params object[] args) { }
    public void LogWarning(string message, params object[] args) { }
    public void LogInformation(string message, params object[] args) { }
}
