using FileLancet.Core.Models;

namespace FileLancet.Core.Interfaces;

/// <summary>
/// 文件解析器通用接口
/// </summary>
public interface IFileLancetParser
{
    /// <summary>
    /// 判断是否可以解析指定文件
    /// </summary>
    bool CanParse(string filePath);

    /// <summary>
    /// 执行文件解析
    /// </summary>
    ParseResult Parse(string filePath);

    /// <summary>
    /// 异步解析文件
    /// </summary>
    Task<ParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default);
}
