namespace FileLancet.Core.Services;

public class ParserFactory
{
    private readonly IEnumerable<IFileLancetParser> _parsers;

    public ParserFactory(IEnumerable<IFileLancetParser> parsers)
    {
        _parsers = parsers;
    }

    public IFileLancetParser? GetParser(string filePath)
    {
        return _parsers.FirstOrDefault(p => p.CanParse(filePath));
    }

    public IEnumerable<IFileLancetParser> GetAllParsers()
    {
        return _parsers;
    }
}
