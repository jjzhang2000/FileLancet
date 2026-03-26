using FileLancet.Core.Utilities;

namespace FileLancet.Core.Tests.Utilities;

/// <summary>
/// 性能优化工具测试 - TC-409 ~ TC-413
/// </summary>
public class PerformanceOptimizerTests : IDisposable
{
    private readonly string _tempDir;

    public PerformanceOptimizerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch { }
    }

    [Fact(DisplayName = "TC-409: 扩展性验证 - 加载 .txt 文件")]
    public async Task Extension_Validation_LoadTxtFile_ShouldWork()
    {
        var filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "Test content\nLine 2\nLine 3");

        // 验证文件可以被正常读取
        var content = await File.ReadAllTextAsync(filePath);

        Assert.NotNull(content);
        Assert.Contains("Test content", content);
    }

    [Fact(DisplayName = "TC-410: 内存使用 - 创建和释放对象")]
    public void MemoryUsage_CreateAndReleaseObjects_ShouldBeStable()
    {
        // 记录初始内存
        var initialMemory = MemoryMonitor.GetCurrentMemoryUsageMB();

        // 创建大量对象
        var list = new List<byte[]>();
        for (int i = 0; i < 100; i++)
        {
            list.Add(new byte[1024 * 1024]); // 1MB each
        }

        // 释放对象
        list.Clear();
        list = null;

        // 强制垃圾回收
        MemoryMonitor.ForceGarbageCollection();

        // 最终内存应该接近初始值（允许一定偏差）
        var finalMemory = MemoryMonitor.GetCurrentMemoryUsageMB();

        // 内存增长不应超过 50MB
        Assert.True(finalMemory - initialMemory < 50, $"内存泄漏: 初始 {initialMemory}MB, 最终 {finalMemory}MB");
    }

    [Fact(DisplayName = "TC-411: 并发性能 - 并行处理任务")]
    public async Task ConcurrentPerformance_ParallelProcessing_ShouldComplete()
    {
        var items = Enumerable.Range(1, 20).ToList();
        var processedCount = 0;

        var results = await PerformanceOptimizer.ParallelProcessAsync(
            items,
            async item =>
            {
                await Task.Delay(10); // 模拟工作
                Interlocked.Increment(ref processedCount);
                return item * 2;
            },
            maxParallel: 4
        );

        Assert.Equal(20, processedCount);
        Assert.Equal(20, results.Count());
    }

    [Fact(DisplayName = "TC-412: 缓存性能 - 缓存命中")]
    public void CachePerformance_CacheHit_ShouldBeFast()
    {
        var cache = new SimpleMemoryCache<string, string>(maxSize: 100);
        var value = "test value";
        cache.Add("key", value);

        // 第一次获取（缓存命中）
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        cache.TryGetValue("key", out var result1);
        stopwatch.Stop();
        var firstAccessTime = stopwatch.ElapsedMilliseconds;

        // 第二次获取（缓存命中）
        stopwatch.Restart();
        cache.TryGetValue("key", out var result2);
        stopwatch.Stop();
        var secondAccessTime = stopwatch.ElapsedMilliseconds;

        Assert.Equal(value, result1);
        Assert.Equal(value, result2);
        // 缓存访问应该非常快（小于 1ms）
        Assert.True(secondAccessTime < 1, $"缓存访问太慢: {secondAccessTime}ms");
    }

    [Fact(DisplayName = "TC-413: 大文件处理 - 分块读取")]
    public async Task LargeFileProcessing_ChunkedReading_ShouldWork()
    {
        var filePath = Path.Combine(_tempDir, "largefile.txt");
        // 创建一个 10MB 的文件
        var content = new string('A', 10 * 1024 * 1024);
        await File.WriteAllTextAsync(filePath, content);

        var totalBytes = 0;
        await PerformanceOptimizer.ReadLargeFileAsync(
            filePath,
            (buffer, count) =>
            {
                totalBytes += count;
                return Task.CompletedTask;
            },
            bufferSize: 81920
        );

        Assert.True(totalBytes > 0);
        Assert.True(totalBytes <= content.Length + 1024); // 允许一定误差
    }

    [Fact(DisplayName = "TC-414: 进度报告 - 处理进度")]
    public async Task ProgressReporting_ShouldReportProgress()
    {
        var items = Enumerable.Range(1, 10).ToList();
        var progressReports = new List<double>();
        var progress = new Progress<double>(p => progressReports.Add(p));

        await PerformanceOptimizer.ProcessWithProgressAsync(
            items,
            async item => await Task.Delay(1),
            progress
        );

        // 等待进度报告完成
        await Task.Delay(100);

        Assert.True(progressReports.Count > 0);
        Assert.Contains(1.0, progressReports); // 最后应该是 100%
    }

    [Fact(DisplayName = "TC-415: 缓存 LRU 策略")]
    public void Cache_LruPolicy_ShouldEvictOldest()
    {
        var cache = new SimpleMemoryCache<string, string>(maxSize: 3);

        cache.Add("key1", "value1");
        cache.Add("key2", "value2");
        cache.Add("key3", "value3");

        // 访问 key1 使其最新
        cache.TryGetValue("key1", out _);

        // 添加 key4，应该淘汰最久未访问的 key2
        cache.Add("key4", "value4");

        Assert.True(cache.TryGetValue("key1", out _));
        Assert.False(cache.TryGetValue("key2", out _)); // 应该被淘汰
        Assert.True(cache.TryGetValue("key3", out _));
        Assert.True(cache.TryGetValue("key4", out _));
    }

    [Fact(DisplayName = "TC-416: 内存监控 - 获取内存使用")]
    public void MemoryMonitor_GetCurrentMemoryUsage_ShouldReturnValue()
    {
        var memory = MemoryMonitor.GetCurrentMemoryUsageMB();

        Assert.True(memory >= 0);
        // 内存使用应该在合理范围内（小于 10GB）
        Assert.True(memory < 10 * 1024);
    }
}
