using FileLancet.Core.Models;

namespace FileLancet.Core.Tests.Models;

public class FileDetailsTests
{
    [Fact]
    public void TC_104_FileDetails_Metadata_CanBeSetAndRetrieved()
    {
        // Arrange
        var details = new FileDetails();
        var authors = new List<string> { "Author 1", "Author 2" };

        // Act
        details.Title = "Test Book";
        details.Authors.AddRange(authors);
        details.Publisher = "Test Publisher";
        details.Language = "en";
        details.Isbn = "1234567890";
        details.Description = "Test Description";
        details.EpubVersion = "3.0";
        details.FilePath = "C:\\test.epub";
        details.FileSize = 1024000;
        details.LastModified = new DateTime(2024, 1, 1);
        details.Checksum = "abc123";

        // Assert
        Assert.Equal("Test Book", details.Title);
        Assert.Equal(2, details.Authors.Count);
        Assert.Equal("Author 1", details.Authors[0]);
        Assert.Equal("Test Publisher", details.Publisher);
        Assert.Equal("en", details.Language);
        Assert.Equal("1234567890", details.Isbn);
        Assert.Equal("Test Description", details.Description);
        Assert.Equal("3.0", details.EpubVersion);
        Assert.Equal("C:\\test.epub", details.FilePath);
        Assert.Equal(1024000, details.FileSize);
        Assert.Equal(new DateTime(2024, 1, 1), details.LastModified);
        Assert.Equal("abc123", details.Checksum);
    }

    [Fact]
    public void TC_104_FileDetails_Statistics_CanBeSet()
    {
        // Arrange
        var details = new FileDetails();

        // Act
        details.ChapterCount = 10;
        details.ImageCount = 5;
        details.StylesheetCount = 2;
        details.TotalFileCount = 50;

        // Assert
        Assert.Equal(10, details.ChapterCount);
        Assert.Equal(5, details.ImageCount);
        Assert.Equal(2, details.StylesheetCount);
        Assert.Equal(50, details.TotalFileCount);
    }

    [Fact]
    public void TC_104_FileDetails_AuthorsList_IsInitialized()
    {
        // Arrange & Act
        var details = new FileDetails();

        // Assert
        Assert.NotNull(details.Authors);
        Assert.Empty(details.Authors);
    }

    [Fact]
    public void TC_104_FileDetails_PublicationDate_CanBeNull()
    {
        // Arrange
        var details = new FileDetails();

        // Act & Assert
        Assert.Null(details.PublicationDate);

        details.PublicationDate = new DateTime(2024, 6, 15);
        Assert.Equal(new DateTime(2024, 6, 15), details.PublicationDate);
    }
}
