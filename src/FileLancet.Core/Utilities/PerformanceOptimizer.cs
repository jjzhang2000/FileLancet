using System.Collections.Concurrent;
using System.IO.Compression;

namespace FileLancet.Core.Utilities;

/// <summary>
/// 性能优化工具类
/// </summary>
public static class PerformanceOptimizer
{
    /// <summary>
    /// 使用 ZIP 流模式（自动释放资源）
    /// </summary>
    public static void UseZipStream(string filePath, Action<ZipArchive> action)
    {
        using var stream = File.OpenRead(filePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        action(archive);
    }

    /// <summary>
    /// 异步使用 ZIP 流模式
    /// </summary>
    public static async Task UseZipStreamAsync(string filePath, Func<ZipArchive, Task> action)
    {
        using var stream = File.OpenRead(filePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        await action(archive);
    }

    /// <summary>
    /// 分块读取大文件
    /// </summary>
    public static async Task ReadLargeFileAsync(string filePath, Func<byte[], int, Task> processChunk, int bufferSize = 81920, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);
        var buffer = new byte[bufferSize];
        int read;

        while ((read = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken)) > 0)
        {
            await processChunk(buffer, read);
        }
    }

    /// <summary>
    /// 并行处理任务（限制并发数）
    /// </summary>
    public static async Task<IEnumerable<TResult>> ParallelProcessAsync<T, TResult>(
        IEnumerable<T> items,
        Func<T, Task<TResult>> processor,
        int maxParallel = 4)
    {
        var semaphore = new SemaphoreSlim(maxParallel);
        var tasks = items.Select(async item =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await processor(item);
            }
            finally
            {
                semaphore.Release();
            }
        });

        return await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 带进度报告的处理
    /// </summary>
    public static async Task ProcessWithProgressAsync<T>(
        IEnumerable<T> items,
        Func<T, Task> processor,
        IProgress<double> progress)
    {
        var itemList = items.ToList();
        int total = itemList.Count;
        int completed = 0;

        foreach (var item in itemList)
        {
            await processor(item);
            completed++;
            progress.Report((double)completed / total);
        }
    }
}

/// <summary>
/// 简单内存缓存（LRU 策略）
/// </summary>
public class SimpleMemoryCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cache;
    private readonly int _maxSize;
    private readonly object _lock = new();

    public SimpleMemoryCache(int maxSize = 100)
    {
        _maxSize = maxSize;
        _cache = new ConcurrentDictionary<TKey, CacheItem<TValue>>();
    }

    /// <summary>
    /// 获取或添加缓存项
    /// </summary>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            item.LastAccessTime = DateTime.UtcNow;
            return item.Value;
        }

        var value = valueFactory(key);
        Add(key, value);
        return value;
    }

    /// <summary>
    /// 尝试获取缓存值
    /// </summary>
    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            item.LastAccessTime = DateTime.UtcNow;
            value = item.Value;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// 添加缓存项
    /// </summary>
    public void Add(TKey key, TValue value)
    {
        lock (_lock)
        {
            // 如果缓存已满，移除最久未访问的项
            if (_cache.Count >= _maxSize)
            {
                var oldestKey = _cache.OrderBy(x => x.Value.LastAccessTime).First().Key;
                _cache.TryRemove(oldestKey, out _);
            }

            _cache[key] = new CacheItem<TValue>(value);
        }
    }

    /// <summary>
    /// 移除缓存项
    /// </summary>
    public bool Remove(TKey key)
    {
        return _cache.TryRemove(key, out _);
    }

    /// <summary>
    /// 清空缓存
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// 获取缓存项数
    /// </summary>
    public int Count => _cache.Count;

    private class CacheItem<T>
    {
        public T Value { get; }
        public DateTime LastAccessTime { get; set; }

        public CacheItem(T value)
        {
            Value = value;
            LastAccessTime = DateTime.UtcNow;
        }
    }
}

/// <summary>
/// 内存使用监控
/// </summary>
public static class MemoryMonitor
{
    /// <summary>
    /// 获取当前内存使用量（MB）
    /// </summary>
    public static long GetCurrentMemoryUsageMB()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return GC.GetTotalMemory(true) / (1024 * 1024);
    }

    /// <summary>
    /// 强制垃圾回收
    /// </summary>
    public static void ForceGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
