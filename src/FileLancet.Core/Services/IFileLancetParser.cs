using FileLancet.Core.Models;

namespace FileLancet.Core.Services;

public interface IFileLancetParser
{
    bool CanParse(string filePath);
    Task<ParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default);
    string[] SupportedExtensions { get; }
    string ParserName { get; }
}
