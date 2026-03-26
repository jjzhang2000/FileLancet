using FileLancet.Core.Models;

namespace FileLancet.Core.Tests.Models;

public class ParseResultTests
{
    [Fact]
    public void TC_105_ParseResult_SuccessState_CanBeSet()
    {
        // Arrange
        var result = new ParseResult();

        // Act
        result.Success = true;
        result.ErrorMessage = "";

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void TC_105_ParseResult_FailureState_CanBeSet()
    {
        // Arrange
        var result = new ParseResult();

        // Act
        result.Success = false;
        result.ErrorMessage = "Parse failed";

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Parse failed", result.ErrorMessage);
    }

    [Fact]
    public void TC_105_ParseResult_RootNode_CanBeSet()
    {
        // Arrange
        var result = new ParseResult();
        var rootNode = new FileNode { Name = "root", Type = NodeType.Root };

        // Act
        result.RootNode = rootNode;

        // Assert
        Assert.NotNull(result.RootNode);
        Assert.Equal("root", result.RootNode.Name);
    }

    [Fact]
    public void TC_105_ParseResult_Details_CanBeSet()
    {
        // Arrange
        var result = new ParseResult();
        var details = new FileDetails { Title = "Test" };

        // Act
        result.Details = details;

        // Assert
        Assert.NotNull(result.Details);
        Assert.Equal("Test", result.Details.Title);
    }

    [Fact]
    public void TC_105_ParseResult_SourcePath_CanBeSet()
    {
        // Arrange
        var result = new ParseResult();

        // Act
        result.SourcePath = "C:\\test.epub";

        // Assert
        Assert.Equal("C:\\test.epub", result.SourcePath);
    }

    [Fact]
    public void TC_105_ParseResult_CompleteSuccessScenario()
    {
        // Arrange
        var result = new ParseResult
        {
            Success = true,
            RootNode = new FileNode { Name = "book.epub", Type = NodeType.Root },
            Details = new FileDetails { Title = "Test Book" },
            SourcePath = "C:\\book.epub"
        };

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RootNode);
        Assert.NotNull(result.Details);
        Assert.Equal("C:\\book.epub", result.SourcePath);
    }

    [Fact]
    public void TC_105_ParseResult_CompleteFailureScenario()
    {
        // Arrange
        var result = new ParseResult
        {
            Success = false,
            ErrorMessage = "File not found",
            SourcePath = "C:\\missing.epub"
        };

        // Assert
        Assert.False(result.Success);
        Assert.Equal("File not found", result.ErrorMessage);
        Assert.Null(result.RootNode);
        Assert.Null(result.Details);
    }
}
