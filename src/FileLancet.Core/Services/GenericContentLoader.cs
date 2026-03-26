using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;

namespace FileLancet.Core.Services;

/// <summary>
/// 通用文件内容加载器 - 用于加载非EPUB文件的内容
/// </summary>
public class GenericContentLoader : IContentLoader
{
    private readonly string _filePath;

    public GenericContentLoader(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    /// <inheritdoc />
    public Task<byte[]> LoadContentAsync(FileNode node)
    {
        // 对于通用文件，直接读取整个文件内容
        // 因为 GenericFileParser 只创建一个根节点，没有子节点
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"File not found: {_filePath}");
        }

        return File.ReadAllBytesAsync(_filePath);
    }

    /// <inheritdoc />
    public async Task<string> LoadTextAsync(FileNode node)
    {
        var bytes = await LoadContentAsync(node);
        // 使用 UTF-8 编码，如果失败则返回空字符串
        try
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public bool ContentExists(FileNode node)
    {
        // 对于通用文件，只要文件存在就认为内容存在
        return File.Exists(_filePath);
    }
}
