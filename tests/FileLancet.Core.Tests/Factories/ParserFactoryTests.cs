using FileLancet.Core.Factories;
using FileLancet.Core.Interfaces;
using FileLancet.Core.Services;

namespace FileLancet.Core.Tests.Factories;

public class ParserFactoryTests : IDisposable
{
    public ParserFactoryTests()
    {
        ParserFactory.ClearParsers();
    }

    public void Dispose()
    {
        ParserFactory.ClearParsers();
    }

    [Fact]
    public void TC_116_RegisterParser_AddsParserToFactory()
    {
        // Arrange
        var parser = new EpubParser();

        // Act
        ParserFactory.RegisterParser(parser);

        // Assert
        var allParsers = ParserFactory.GetAllParsers();
        Assert.Single(allParsers);
        Assert.Contains(parser, allParsers);
    }

    [Fact]
    public void TC_116_RegisterParser_NullParser_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ParserFactory.RegisterParser(null!));
    }

    [Fact]
    public void TC_116_RegisterParser_MultipleParsers_AreAllAdded()
    {
        // Arrange
        var parser1 = new EpubParser();
        var parser2 = new TestParser();

        // Act
        ParserFactory.RegisterParser(parser1);
        ParserFactory.RegisterParser(parser2);

        // Assert
        var allParsers = ParserFactory.GetAllParsers();
        Assert.Equal(2, allParsers.Count);
    }

    [Fact]
    public void TC_116_GetParser_WithMatchingParser_ReturnsParser()
    {
        // Arrange
        var testParser = new TestParser { CanParseValue = true };
        ParserFactory.RegisterParser(testParser);

        // Act
        var result = ParserFactory.GetParser("test.epub");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TestParser>(result);
    }

    [Fact]
    public void TC_116_GetParser_NoMatchingParser_ReturnsNull()
    {
        // Arrange
        ParserFactory.RegisterParser(new TestParser());

        // Act
        var result = ParserFactory.GetParser("test.epub");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TC_116_GetParser_EmptyPath_ReturnsNull()
    {
        // Arrange
        ParserFactory.RegisterParser(new TestParser { CanParseValue = true });

        // Act
        var result = ParserFactory.GetParser("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TC_116_GetParser_NullPath_ReturnsNull()
    {
        // Arrange
        ParserFactory.RegisterParser(new TestParser { CanParseValue = true });

        // Act
        var result = ParserFactory.GetParser(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TC_116_GetAllParsers_ReturnsReadOnlyList()
    {
        // Arrange
        ParserFactory.RegisterParser(new TestParser());

        // Act
        var result = ParserFactory.GetAllParsers();

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IReadOnlyList<IFileLancetParser>>(result);
    }

    [Fact]
    public void TC_116_ClearParsers_RemovesAllParsers()
    {
        // Arrange
        ParserFactory.RegisterParser(new EpubParser());
        ParserFactory.RegisterParser(new TestParser());

        // Act
        ParserFactory.ClearParsers();

        // Assert
        Assert.Empty(ParserFactory.GetAllParsers());
    }

    [Fact]
    public void TC_116_InitializeDefaults_RegistersEpubParser()
    {
        // Act
        ParserFactory.InitializeDefaults();

        // Assert
        var allParsers = ParserFactory.GetAllParsers();
        Assert.Single(allParsers);
        Assert.IsType<EpubParser>(allParsers[0]);
    }

    [Fact]
    public void TC_116_InitializeDefaults_ClearsExistingParsers()
    {
        // Arrange
        ParserFactory.RegisterParser(new TestParser());

        // Act
        ParserFactory.InitializeDefaults();

        // Assert
        var allParsers = ParserFactory.GetAllParsers();
        Assert.Single(allParsers);
        Assert.IsType<EpubParser>(allParsers[0]);
    }

    [Fact]
    public void TC_116_Factory_IsThreadSafe()
    {
        // Arrange
        var parsers = new List<IFileLancetParser>();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var parser = new TestParser { CanParseValue = i % 2 == 0 };
            parsers.Add(parser);
            tasks.Add(Task.Run(() => ParserFactory.RegisterParser(parser)));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.Equal(10, ParserFactory.GetAllParsers().Count);
    }

    private class TestParser : IFileLancetParser
    {
        public bool CanParseValue { get; set; }

        public bool CanParse(string filePath)
        {
            return CanParseValue;
        }

        public FileLancet.Core.Models.ParseResult Parse(string filePath)
        {
            return new FileLancet.Core.Models.ParseResult { Success = true };
        }

        public Task<FileLancet.Core.Models.ParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Parse(filePath));
        }
    }
}
