using FileLancet.Core.Models;

namespace FileLancet.Core.Interfaces;

/// <summary>
/// 内容加载器接口
/// </summary>
public interface IContentLoader
{
    /// <summary>
    /// 异步加载节点内容
    /// </summary>
    Task<byte[]> LoadContentAsync(FileNode node);

    /// <summary>
    /// 异步加载文本内容
    /// </summary>
    Task<string> LoadTextAsync(FileNode node);

    /// <summary>
    /// 检查节点内容是否存在
    /// </summary>
    bool ContentExists(FileNode node);
}

/// <summary>
/// 内容加载器工厂
/// </summary>
public interface IContentLoaderFactory
{
    /// <summary>
    /// 为指定 EPUB 文件创建内容加载器
    /// </summary>
    IContentLoader CreateLoader(string epubPath);
}
