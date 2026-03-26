using FileLancet.Core.Models;

namespace FileLancet.Core.Tests.Models;

public class FileNodeTests
{
    [Fact]
    public void TC_101_FileNode_Properties_CanBeSetAndGet()
    {
        // Arrange
        var node = new FileNode();
        
        // Act
        node.Name = "test.txt";
        node.Path = "chapter1/test.txt";
        node.Type = NodeType.Html;
        node.Size = 1024;
        node.MimeType = "text/html";
        node.Description = "测试文件";
        node.IsContentLoaded = true;
        
        // Assert
        Assert.Equal("test.txt", node.Name);
        Assert.Equal("chapter1/test.txt", node.Path);
        Assert.Equal(NodeType.Html, node.Type);
        Assert.Equal(1024, node.Size);
        Assert.Equal("text/html", node.MimeType);
        Assert.Equal("测试文件", node.Description);
        Assert.True(node.IsContentLoaded);
    }

    [Fact]
    public void TC_102_FileNode_ParentChildRelationship_IsCorrect()
    {
        // Arrange
        var parent = new FileNode { Name = "parent", Type = NodeType.Folder };
        var child = new FileNode { Name = "child", Type = NodeType.Html };
        
        // Act
        parent.AddChild(child);
        
        // Assert
        Assert.Single(parent.Children);
        Assert.Equal(child, parent.Children[0]);
        Assert.Equal(parent, child.Parent);
    }

    [Fact]
    public void TC_103_FileNode_GetFullPath_ReturnsCorrectPath()
    {
        // Arrange
        var root = new FileNode { Name = "root", Path = "" };
        var folder = new FileNode { Name = "OEBPS", Path = "OEBPS" };
        var file = new FileNode { Name = "chapter1.xhtml", Path = "chapter1.xhtml" };
        
        root.AddChild(folder);
        folder.AddChild(file);
        
        // Act & Assert
        Assert.Equal("", root.GetFullPath());
        Assert.Equal("OEBPS", folder.GetFullPath());
        // 使用 Path.Combine 会返回平台特定的分隔符
        var expectedPath = System.IO.Path.Combine("OEBPS", "chapter1.xhtml");
        Assert.Equal(expectedPath, file.GetFullPath());
    }

    [Fact]
    public void TC_103_FileNode_GetFullPath_WithoutParent_ReturnsPath()
    {
        // Arrange
        var node = new FileNode { Path = "test/path" };
        
        // Act
        var fullPath = node.GetFullPath();
        
        // Assert
        Assert.Equal("test/path", fullPath);
    }

    [Fact]
    public void TC_102_FileNode_MultipleChildren_AreAddedCorrectly()
    {
        // Arrange
        var parent = new FileNode { Name = "parent" };
        var child1 = new FileNode { Name = "child1" };
        var child2 = new FileNode { Name = "child2" };
        
        // Act
        parent.AddChild(child1);
        parent.AddChild(child2);
        
        // Assert
        Assert.Equal(2, parent.Children.Count);
        Assert.Equal(parent, child1.Parent);
        Assert.Equal(parent, child2.Parent);
    }
}
