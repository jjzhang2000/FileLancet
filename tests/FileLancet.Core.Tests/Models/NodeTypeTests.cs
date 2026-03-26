using FileLancet.Core.Models;

namespace FileLancet.Core.Tests.Models;

public class NodeTypeTests
{
    [Theory]
    [InlineData(NodeType.Root, 0)]
    [InlineData(NodeType.Folder, 1)]
    [InlineData(NodeType.Container, 2)]
    [InlineData(NodeType.Opf, 3)]
    [InlineData(NodeType.Ncx, 4)]
    [InlineData(NodeType.Nav, 5)]
    [InlineData(NodeType.Html, 6)]
    [InlineData(NodeType.Css, 7)]
    [InlineData(NodeType.Image, 8)]
    [InlineData(NodeType.Font, 9)]
    [InlineData(NodeType.Audio, 10)]
    [InlineData(NodeType.Video, 11)]
    [InlineData(NodeType.Script, 12)]
    [InlineData(NodeType.Other, 13)]
    public void TC_106_NodeType_EnumValues_AreDefined(NodeType type, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)type);
    }

    [Fact]
    public void TC_106_NodeType_AllValues_CanBeUsedInSwitch()
    {
        // Arrange
        var types = Enum.GetValues<NodeType>();

        // Act & Assert
        foreach (var type in types)
        {
            var result = GetTypeName(type);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }

    private static string GetTypeName(NodeType type) => type switch
    {
        NodeType.Root => "Root",
        NodeType.Folder => "Folder",
        NodeType.Container => "Container",
        NodeType.Opf => "Opf",
        NodeType.Ncx => "Ncx",
        NodeType.Nav => "Nav",
        NodeType.Html => "Html",
        NodeType.Css => "Css",
        NodeType.Image => "Image",
        NodeType.Font => "Font",
        NodeType.Audio => "Audio",
        NodeType.Video => "Video",
        NodeType.Script => "Script",
        NodeType.Other => "Other",
        _ => throw new ArgumentOutOfRangeException()
    };
}
