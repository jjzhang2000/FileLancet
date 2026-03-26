using System.IO.Compression;
using System.Text;
using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;

namespace FileLancet.Core.Services;

/// <summary>
/// EPUB 内容加载器实现
/// </summary>
public class EpubContentLoader : IContentLoader
{
    private readonly string _epubPath;
    private readonly Dictionary<string, byte[]> _contentCache;
    private readonly SemaphoreSlim _cacheLock;
    private ZipArchive? _archive;
    private readonly object _archiveLock = new();

    public EpubContentLoader(string epubPath)
    {
        _epubPath = epubPath ?? throw new ArgumentNullException(nameof(epubPath));
        _contentCache = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        _cacheLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// 获取或创建 ZIP 归档实例
    /// </summary>
    private ZipArchive GetArchive()
    {
        lock (_archiveLock)
        {
            if (_archive == null)
            {
                var stream = File.OpenRead(_epubPath);
                _archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
            }
            return _archive;
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> LoadContentAsync(FileNode node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        var path = node.Path.Replace("\\", "/");

        // 检查缓存
        await _cacheLock.WaitAsync();
        try
        {
            if (_contentCache.TryGetValue(path, out var cachedContent))
            {
                return cachedContent;
            }
        }
        finally
        {
            _cacheLock.Release();
        }

        // 从 ZIP 加载
        var archive = GetArchive();
        var entry = archive.GetEntry(path);

        if (entry == null)
            throw new FileNotFoundException($"Entry not found: {path}");

        using var entryStream = entry.Open();
        using var memoryStream = new MemoryStream();
        await entryStream.CopyToAsync(memoryStream);
        var content = memoryStream.ToArray();

        // 存入缓存
        await _cacheLock.WaitAsync();
        try
        {
            _contentCache[path] = content;
        }
        finally
        {
            _cacheLock.Release();
        }

        return content;
    }

    /// <inheritdoc />
    public async Task<string> LoadTextAsync(FileNode node)
    {
        var bytes = await LoadContentAsync(node);

        // 尝试检测编码
        Encoding encoding = DetectEncoding(bytes);
        return encoding.GetString(bytes);
    }

    /// <inheritdoc />
    public bool ContentExists(FileNode node)
    {
        if (node == null)
            return false;

        var path = node.Path.Replace("\\", "/");

        // 检查缓存
        if (_contentCache.ContainsKey(path))
            return true;

        // 检查 ZIP
        try
        {
            var archive = GetArchive();
            return archive.GetEntry(path) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 从缓存获取内容
    /// </summary>
    public bool TryGetFromCache(string path, out byte[]? content)
    {
        return _contentCache.TryGetValue(path.Replace("\\", "/"), out content);
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public void ClearCache()
    {
        _cacheLock.Wait();
        try
        {
            _contentCache.Clear();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        lock (_archiveLock)
        {
            _archive?.Dispose();
            _archive = null;
        }
        _cacheLock.Dispose();
    }

    /// <summary>
    /// 检测字节数组的编码
    /// </summary>
    private static Encoding DetectEncoding(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8;

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode;

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode;

        // 默认 UTF-8
        return Encoding.UTF8;
    }
}

/// <summary>
/// 内容加载器工厂实现
/// </summary>
public class ContentLoaderFactory : IContentLoaderFactory
{
    public IContentLoader CreateLoader(string epubPath)
    {
        return new EpubContentLoader(epubPath);
    }
}
