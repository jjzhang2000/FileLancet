using FileLancet.Core.Factories;
using FileLancet.Core.Models;
using FileLancet.Core.Services;

namespace FileLancet.Core.Tests.Services;

/// <summary>
/// BaseParser 和 PlainTextParser 测试 - TC-401 ~ TC-408
/// </summary>
public class BaseParserTests : IDisposable
{
    private readonly string _tempDir;

    public BaseParserTests()
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

    [Fact(DisplayName = "TC-401: BaseParser 异常处理")]
    public void BaseParser_HandleException_ShouldReturnErrorResult()
    {
        var parser = new TestParser();
        var exception = new InvalidOperationException("测试异常");
        var filePath = "test.txt";

        var result = parser.TestHandleException(exception, filePath);

        Assert.False(result.Success);
        Assert.Contains("测试异常", result.ErrorMessage);
        Assert.Equal(filePath, result.SourcePath);
    }

    [Fact(DisplayName = "TC-402: BaseParser 节点创建")]
    public void BaseParser_CreateNode_ShouldCreateNodeWithCorrectProperties()
    {
        var parser = new TestParser();

        var node = parser.TestCreateNode("test", "path", NodeType.Root);

        Assert.Equal("test", node.Name);
        Assert.Equal("path", node.Path);
        Assert.Equal(NodeType.Root, node.Type);
        Assert.NotNull(node.Description);
    }

    [Fact(DisplayName = "TC-403: PlainTextParser CanParse - .txt 文件")]
    public void PlainTextParser_CanParse_TxtFile_ShouldReturnTrue()
    {
        var parser = new PlainTextParser();

        Assert.True(parser.CanParse("test.txt"));
        Assert.True(parser.CanParse("test.LOG"));
        Assert.True(parser.CanParse("test.md"));
    }

    [Fact(DisplayName = "TC-404: PlainTextParser CanParse - 非 .txt 文件")]
    public void PlainTextParser_CanParse_NonTxtFile_ShouldReturnFalse()
    {
        var parser = new PlainTextParser();

        Assert.False(parser.CanParse("test.epub"));
        Assert.False(parser.CanParse("test.pdf"));
        Assert.False(parser.CanParse("test"));
    }

    [Fact(DisplayName = "TC-405: PlainTextParser Parse - 有效 txt")]
    public void PlainTextParser_Parse_ValidTxt_ShouldReturnSuccess()
    {
        var parser = new PlainTextParser();
        var filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "Line 1\nLine 2\nLine 3");

        var result = parser.Parse(filePath);

        Assert.True(result.Success);
        Assert.NotNull(result.RootNode);
        Assert.NotNull(result.Details);
        Assert.Equal("test.txt", result.RootNode.Name);
    }

    [Fact(DisplayName = "TC-406: PlainTextParser Parse - 无效路径")]
    public void PlainTextParser_Parse_InvalidPath_ShouldReturnError()
    {
        var parser = new PlainTextParser();
        var filePath = Path.Combine(_tempDir, "nonexistent.txt");

        var result = parser.Parse(filePath);

        Assert.False(result.Success);
        Assert.Contains("不存在", result.ErrorMessage);
    }

    [Fact(DisplayName = "TC-407: 工厂注册新解析器")]
    public void ParserFactory_RegisterPlainTextParser_ShouldContainNewParser()
    {
        ParserFactory.ClearParsers();
        var parser = new PlainTextParser();

        ParserFactory.RegisterParser(parser);

        var allParsers = ParserFactory.GetAllParsers();
        Assert.Contains(parser, allParsers);
    }

    [Fact(DisplayName = "TC-408: 工厂获取解析器 - .txt 文件")]
    public void ParserFactory_GetParser_TxtFile_ShouldReturnPlainTextParser()
    {
        ParserFactory.ClearParsers();
        ParserFactory.RegisterParser(new PlainTextParser());
        var filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "test");

        var parser = ParserFactory.GetParser(filePath);

        Assert.IsType<PlainTextParser>(parser);
    }

    /// <summary>
    /// 测试用解析器实现
    /// </summary>
    private class TestParser : BaseParser
    {
        public TestParser() : base(null) { }

        public override bool CanParse(string filePath) => true;

        public override ParseResult Parse(string filePath)
        {
            return new ParseResult { Success = true };
        }

        protected override string GetNodeDescription(NodeType type) => "测试节点";

        // 暴露受保护方法用于测试
        public ParseResult TestHandleException(Exception ex, string filePath) => HandleException(ex, filePath);
        public FileNode TestCreateNode(string name, string path, NodeType type) => CreateNode(name, path, type);
    }
}
