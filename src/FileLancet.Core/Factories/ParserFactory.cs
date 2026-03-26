using FileLancet.Core.Interfaces;
using FileLancet.Core.Services;

namespace FileLancet.Core.Factories;

/// <summary>
/// 解析器工厂
/// </summary>
public static class ParserFactory
{
    private static readonly List<IFileLancetParser> _parsers = new();
    private static readonly object _lock = new();

    /// <summary>
    /// 注册解析器
    /// </summary>
    public static void RegisterParser(IFileLancetParser parser)
    {
        if (parser == null)
            throw new ArgumentNullException(nameof(parser));

        lock (_lock)
        {
            _parsers.Add(parser);
        }
    }

    /// <summary>
    /// 获取匹配的解析器
    /// </summary>
    public static IFileLancetParser? GetParser(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return null;

        lock (_lock)
        {
            return _parsers.FirstOrDefault(p => p.CanParse(filePath));
        }
    }

    /// <summary>
    /// 获取所有已注册的解析器
    /// </summary>
    public static IReadOnlyList<IFileLancetParser> GetAllParsers()
    {
        lock (_lock)
        {
            return _parsers.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// 清除所有解析器
    /// </summary>
    public static void ClearParsers()
    {
        lock (_lock)
        {
            _parsers.Clear();
        }
    }

    /// <summary>
    /// 初始化默认解析器
    /// </summary>
    public static void InitializeDefaults()
    {
        lock (_lock)
        {
            _parsers.Clear();
            RegisterParser(new EpubParser());
            RegisterParser(new PlainTextParser());
            // 通用解析器作为兜底，必须最后注册
            RegisterParser(new GenericFileParser());
        }
    }
}
