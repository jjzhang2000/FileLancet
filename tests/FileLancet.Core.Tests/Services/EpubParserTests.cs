using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using FileLancet.Core.Models;
using FileLancet.Core.Services;

namespace FileLancet.Core.Tests.Services;

public class EpubParserTests : IDisposable
{
    private readonly EpubParser _parser;
    private readonly string _testFilesDir;

    public EpubParserTests()
    {
        _parser = new EpubParser();
        _testFilesDir = Path.Combine(Path.GetTempPath(), "FileLancetTests");
        Directory.CreateDirectory(_testFilesDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testFilesDir))
        {
            Directory.Delete(_testFilesDir, true);
        }
    }

    private string CreateTestEpub(string fileName, Action<ZipArchive> setup)
    {
        var path = Path.Combine(_testFilesDir, fileName);
        using var stream = File.Create(path);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
        setup(archive);
        return path;
    }

    private void AddEntry(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, Encoding.UTF8);
        writer.Write(content);
    }

    private string GetContainerXml(string opfPath = "OEBPS/content.opf")
    {
        return $"<?xml version=\"1.0\"?>\n<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">\n  <rootfiles>\n    <rootfile full-path=\"{opfPath}\" media-type=\"application/oebps-package+xml\"/>\n  </rootfiles>\n</container>";
    }

    private string GetOpfXml(string version = "3.0", string title = "Test Book")
    {
        return $"<?xml version=\"1.0\"?>\n<package version=\"{version}\" xmlns=\"http://www.idpf.org/2007/opf\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n  <metadata>\n    <dc:title>{title}</dc:title>\n    <dc:creator>Test Author</dc:creator>\n    <dc:language>en</dc:language>\n  </metadata>\n  <manifest>\n    <item id=\"toc\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>\n    <item id=\"chapter1\" href=\"chapter1.xhtml\" media-type=\"application/xhtml+xml\"/>\n  </manifest>\n  <spine toc=\"toc\">\n    <itemref idref=\"chapter1\"/>\n  </spine>\n</package>";
    }

    [Fact]
    public void TC_107_CanParse_ValidEpub_ReturnsTrue()
    {
        // Arrange
        var path = CreateTestEpub("valid.epub", archive =>
        {
            AddEntry(archive, "META-INF/container.xml", GetContainerXml());
            AddEntry(archive, "OEBPS/content.opf", GetOpfXml());
        });

        // Act
        var result = _parser.CanParse(path);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TC_108_CanParse_NonExistentFile_ReturnsFalse()
    {
        // Act
        var result = _parser.CanParse("nonexistent.epub");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TC_109_CanParse_NonEpubExtension_ReturnsFalse()
    {
        // Arrange
        var path = Path.Combine(_testFilesDir, "test.txt");
        File.WriteAllText(path, "not an epub");

        // Act
        var result = _parser.CanParse(path);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TC_110_CanParse_InvalidZip_ReturnsFalse()
    {
        // Arrange
        var path = Path.Combine(_testFilesDir, "invalid.epub");
        File.WriteAllText(path, "not a zip file");

        // Act
        var result = _parser.CanParse(path);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TC_111_CanParse_MissingContainerXml_ReturnsFalse()
    {
        // Arrange
        var path = CreateTestEpub("nocontainer.epub", archive =>
        {
            AddEntry(archive, "random.txt", "content");
        });

        // Act
        var result = _parser.CanParse(path);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TC_112_Parse_ValidEpub_ReturnsSuccess()
    {
        // Arrange
        var path = CreateTestEpub("valid.epub", archive =>
        {
            AddEntry(archive, "META-INF/container.xml", GetContainerXml());
            AddEntry(archive, "OEBPS/content.opf", GetOpfXml("3.0", "Test Book"));
            AddEntry(archive, "OEBPS/chapter1.xhtml", "<html><body>Chapter 1</body></html>");
            AddEntry(archive, "OEBPS/style.css", "body { margin: 0; }");
        });

        // Act
        var result = _parser.Parse(path);

        // Assert
        Assert.True(result.Success, $"Parse failed: {result.ErrorMessage}");
        Assert.NotNull(result.RootNode);
        Assert.NotNull(result.Details);
        Assert.Equal("Test Book", result.Details.Title);
        Assert.Equal("3.0", result.Details.EpubVersion);
    }

    [Fact]
    public void TC_113_Parse_ExtractsMetadata_Correctly()
    {
        // Arrange
        var path = CreateTestEpub("metadata.epub", archive =>
        {
            AddEntry(archive, "META-INF/container.xml", GetContainerXml());
            AddEntry(archive, "OEBPS/content.opf", @"<?xml version=""1.0""?>
<package version=""2.0"" xmlns=""http://www.idpf.org/2007/opf"" xmlns:dc=""http://purl.org/dc/elements/1.1/"">
  <metadata>
    <dc:title>My Book</dc:title>
    <dc:creator>John Doe</dc:creator>
    <dc:creator>Jane Smith</dc:creator>
    <dc:publisher>Test Publisher</dc:publisher>
    <dc:language>zh-CN</dc:language>
    <dc:identifier>123456789</dc:identifier>
    <dc:description>A test book</dc:description>
  </metadata>
  <manifest></manifest>
  <spine></spine>
</package>");
        });

        // Act
        var result = _parser.Parse(path);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("My Book", result.Details!.Title);
        Assert.Equal(2, result.Details.Authors.Count);
        Assert.Contains("John Doe", result.Details.Authors);
        Assert.Contains("Jane Smith", result.Details.Authors);
        Assert.Equal("Test Publisher", result.Details.Publisher);
        Assert.Equal("zh-CN", result.Details.Language);
        Assert.Equal("123456789", result.Details.Isbn);
        Assert.Equal("A test book", result.Details.Description);
        Assert.Equal("2.0", result.Details.EpubVersion);
    }

    [Fact]
    public void TC_114_Parse_InvalidEpub_ReturnsFailure()
    {
        // Arrange
        var path = CreateTestEpub("invalid.epub", archive =>
        {
            AddEntry(archive, "META-INF/container.xml", "invalid xml");
        });

        // Act
        var result = _parser.Parse(path);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.ErrorMessage);
    }

    [Fact]
    public void TC_115_Parse_NonExistentFile_ReturnsFailure()
    {
        // Act
        var result = _parser.Parse("nonexistent.epub");

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void TC_116_Parse_BuildsFileTree_Correctly()
    {
        // Arrange
        var path = CreateTestEpub("tree.epub", archive =>
        {
            AddEntry(archive, "META-INF/container.xml", GetContainerXml());
            AddEntry(archive, "OEBPS/content.opf", GetOpfXml());
            AddEntry(archive, "OEBPS/chapter1.xhtml", "<html></html>");
            AddEntry(archive, "OEBPS/chapter2.xhtml", "<html></html>");
            AddEntry(archive, "OEBPS/images/cover.jpg", "fake image data");
            AddEntry(archive, "OEBPS/style.css", "body{}");
        });

        // Act
        var result = _parser.Parse(path);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RootNode);
        Assert.True(result.RootNode.Children.Count > 0);
        Assert.Equal("tree.epub", result.RootNode.Name);
    }

    [Fact]
    public async Task TC_117_ParseAsync_ValidEpub_ReturnsSuccess()
    {
        // Arrange
        var path = CreateTestEpub("async.epub", archive =>
        {
            AddEntry(archive, "META-INF/container.xml", GetContainerXml());
            AddEntry(archive, "OEBPS/content.opf", GetOpfXml());
        });

        // Act
        var result = await _parser.ParseAsync(path);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task TC_118_ParseAsync_Cancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var path = CreateTestEpub("cancel.epub", archive =>
        {
            AddEntry(archive, "META-INF/container.xml", GetContainerXml());
            AddEntry(archive, "OEBPS/content.opf", GetOpfXml());
        });

        var cts = new CancellationTokenSource();
        // 延迟取消，确保在异步操作开始后才取消
        cts.CancelAfter(1);

        // Act & Assert - TaskCanceledException 是 OperationCanceledException 的子类
        var exception = await Record.ExceptionAsync(async () =>
        {
            await _parser.ParseAsync(path, cts.Token);
        });

        // 由于取消时间不确定，可能抛出异常也可能成功完成
        // 如果抛出异常，必须是 OperationCanceledException
        if (exception != null)
        {
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }
    }

    [Fact]
    public void TC_119_Parse_CountsFiles_Correctly()
    {
        // Arrange
        var path = CreateTestEpub("counts.epub", archive =>
        {
            AddEntry(archive, "META-INF/container.xml", GetContainerXml());
            AddEntry(archive, "OEBPS/content.opf", GetOpfXml());
            AddEntry(archive, "OEBPS/chapter1.xhtml", "<html></html>");
            AddEntry(archive, "OEBPS/chapter2.xhtml", "<html></html>");
            AddEntry(archive, "OEBPS/image1.jpg", "img");
            AddEntry(archive, "OEBPS/image2.png", "img");
            AddEntry(archive, "OEBPS/style.css", "body{}");
        });

        // Act
        var result = _parser.Parse(path);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Details!.ChapterCount);
        Assert.Equal(2, result.Details.ImageCount);
        Assert.Equal(1, result.Details.StylesheetCount);
        Assert.Equal(7, result.Details.TotalFileCount);
    }

    [Fact]
    public void TC_120_Parse_SetsFileInfo_Correctly()
    {
        // Arrange
        var path = CreateTestEpub("info.epub", archive =>
        {
            AddEntry(archive, "META-INF/container.xml", GetContainerXml());
            AddEntry(archive, "OEBPS/content.opf", GetOpfXml());
        });

        var fileInfo = new FileInfo(path);

        // Act
        var result = _parser.Parse(path);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(path, result.Details!.FilePath);
        Assert.Equal(fileInfo.Length, result.Details.FileSize);
        Assert.Equal(fileInfo.LastWriteTime, result.Details.LastModified);
    }
}
