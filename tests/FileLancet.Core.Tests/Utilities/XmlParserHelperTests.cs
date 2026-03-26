using System.IO.Compression;
using System.Xml.Linq;
using FileLancet.Core.Utilities;

namespace FileLancet.Core.Tests.Utilities;

/// <summary>
/// XmlParserHelper 测试
/// </summary>
public class XmlParserHelperTests : IDisposable
{
    private readonly string _tempDir;

    public XmlParserHelperTests()
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

    [Fact(DisplayName = "TC-501: ParseWithNamespaceFix - 正常 XML")]
    public void ParseWithNamespaceFix_ValidXml_ShouldReturnDocument()
    {
        var xml = "<?xml version=\"1.0\"?><package xmlns=\"http://www.idpf.org/2007/opf\"><metadata><title>Test</title></metadata></package>";

        var doc = XmlParserHelper.ParseWithNamespaceFix(xml);

        Assert.NotNull(doc);
        Assert.Equal("package", doc.Root?.Name.LocalName);
    }

    [Fact(DisplayName = "TC-502: ParseWithNamespaceFix - 包含未声明 dc 前缀")]
    public void ParseWithNamespaceFix_UndeclaredDcPrefix_ShouldFixAndParse()
    {
        var xml = "<?xml version=\"1.0\"?><package><metadata><dc:title>Test Title</dc:title><dc:creator>Author</dc:creator></metadata></package>";

        var doc = XmlParserHelper.ParseWithNamespaceFix(xml);

        Assert.NotNull(doc);
        var title = XmlParserHelper.GetElementValueByLocalName(doc, "title");
        Assert.Equal("Test Title", title);
    }

    [Fact(DisplayName = "TC-503: ParseWithNamespaceFix - 已声明 dc 前缀")]
    public void ParseWithNamespaceFix_DeclaredDcPrefix_ShouldNotDuplicate()
    {
        var xml = "<?xml version=\"1.0\"?><package xmlns:dc=\"http://purl.org/dc/elements/1.1/\"><metadata><dc:title>Test</dc:title></metadata></package>";

        var doc = XmlParserHelper.ParseWithNamespaceFix(xml);

        Assert.NotNull(doc);
        // 确保不会添加重复的命名空间声明
        var packageElement = doc.Root;
        Assert.NotNull(packageElement);
    }

    [Fact(DisplayName = "TC-504: ParseWithNamespaceFix - 空内容应抛出异常")]
    public void ParseWithNamespaceFix_EmptyContent_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => XmlParserHelper.ParseWithNamespaceFix(""));
        Assert.Throws<ArgumentException>(() => XmlParserHelper.ParseWithNamespaceFix("   "));
    }

    [Fact(DisplayName = "TC-505: GetZipEntry - 正斜杠路径")]
    public void GetZipEntry_ForwardSlashPath_ShouldFindEntry()
    {
        var zipPath = CreateTestZip(new[] { ("META-INF/container.xml", "<container/>") });

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = XmlParserHelper.GetZipEntry(archive, "META-INF/container.xml");

        Assert.NotNull(entry);
    }

    [Fact(DisplayName = "TC-506: GetZipEntry - 反斜杠路径")]
    public void GetZipEntry_BackslashPath_ShouldFindEntry()
    {
        var zipPath = CreateTestZip(new[] { ("META-INF\\container.xml", "<container/>") });

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = XmlParserHelper.GetZipEntry(archive, "META-INF/container.xml");

        Assert.NotNull(entry);
    }

    [Fact(DisplayName = "TC-507: GetZipEntry - 不区分大小写")]
    public void GetZipEntry_CaseInsensitive_ShouldFindEntry()
    {
        var zipPath = CreateTestZip(new[] { ("META-INF/CONTAINER.XML", "<container/>") });

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = XmlParserHelper.GetZipEntry(archive, "meta-inf/container.xml");

        Assert.NotNull(entry);
    }

    [Fact(DisplayName = "TC-508: GetZipEntry - 不存在路径")]
    public void GetZipEntry_NonExistentPath_ShouldReturnNull()
    {
        var zipPath = CreateTestZip(new[] { ("other/file.txt", "content") });

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = XmlParserHelper.GetZipEntry(archive, "nonexistent/file.xml");

        Assert.Null(entry);
    }

    [Fact(DisplayName = "TC-509: GetElementsByLocalName - 查找多个元素")]
    public void GetElementsByLocalName_MultipleElements_ShouldReturnAll()
    {
        var xml = "<root><item>1</item><item>2</item><item>3</item></root>";
        var doc = XDocument.Parse(xml);

        var items = XmlParserHelper.GetElementsByLocalName(doc, "item").ToList();

        Assert.Equal(3, items.Count);
        Assert.Equal("1", items[0].Value);
        Assert.Equal("2", items[1].Value);
        Assert.Equal("3", items[2].Value);
    }

    [Fact(DisplayName = "TC-510: GetFirstElementByLocalName - 查找第一个")]
    public void GetFirstElementByLocalName_FirstElement_ShouldReturnFirst()
    {
        var xml = "<root><item>1</item><item>2</item></root>";
        var doc = XDocument.Parse(xml);

        var item = XmlParserHelper.GetFirstElementByLocalName(doc, "item");

        Assert.NotNull(item);
        Assert.Equal("1", item.Value);
    }

    [Fact(DisplayName = "TC-511: GetFirstElementByLocalName - 不存在元素")]
    public void GetFirstElementByLocalName_NonExistent_ShouldReturnNull()
    {
        var xml = "<root><item>1</item></root>";
        var doc = XDocument.Parse(xml);

        var element = XmlParserHelper.GetFirstElementByLocalName(doc, "nonexistent");

        Assert.Null(element);
    }

    [Fact(DisplayName = "TC-512: GetElementValueByLocalName - 获取值")]
    public void GetElementValueByLocalName_ExistingElement_ShouldReturnValue()
    {
        var xml = "<root><title>Test Title</title></root>";
        var doc = XDocument.Parse(xml);

        var value = XmlParserHelper.GetElementValueByLocalName(doc, "title");

        Assert.Equal("Test Title", value);
    }

    [Fact(DisplayName = "TC-513: GetElementValueByLocalName - 不存在返回 null")]
    public void GetElementValueByLocalName_NonExistent_ShouldReturnNull()
    {
        var xml = "<root><title>Test</title></root>";
        var doc = XDocument.Parse(xml);

        var value = XmlParserHelper.GetElementValueByLocalName(doc, "nonexistent");

        Assert.Null(value);
    }

    [Fact(DisplayName = "TC-514: LoadFromZipEntryAsync - 正常加载")]
    public async Task LoadFromZipEntryAsync_ValidEntry_ShouldLoadDocument()
    {
        var zipPath = CreateTestZip(new[] { ("test.xml", "<?xml version=\"1.0\"?><root><item>value</item></root>") });

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry("test.xml");
        Assert.NotNull(entry);

        var doc = await XmlParserHelper.LoadFromZipEntryAsync(entry);

        Assert.NotNull(doc);
        Assert.Equal("root", doc.Root?.Name.LocalName);
    }

    [Fact(DisplayName = "TC-515: LoadFromZipEntryAsync - 包含未声明命名空间")]
    public async Task LoadFromZipEntryAsync_UndeclaredNamespace_ShouldFixAndLoad()
    {
        var xml = "<?xml version=\"1.0\"?><package><metadata><dc:title>Test</dc:title></metadata></package>";
        var zipPath = CreateTestZip(new[] { ("content.opf", xml) });

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry("content.opf");
        Assert.NotNull(entry);

        var doc = await XmlParserHelper.LoadFromZipEntryAsync(entry);

        Assert.NotNull(doc);
        var title = XmlParserHelper.GetElementValueByLocalName(doc, "title");
        Assert.Equal("Test", title);
    }

    [Fact(DisplayName = "TC-516: LoadFromZipEntryAsync - 取消操作")]
    public async Task LoadFromZipEntryAsync_Cancellation_ShouldThrowOperationCanceledException()
    {
        var zipPath = CreateTestZip(new[] { ("test.xml", "<root/>") });

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry("test.xml");
        Assert.NotNull(entry);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            XmlParserHelper.LoadFromZipEntryAsync(entry, cts.Token));
    }

    private string CreateTestZip(IEnumerable<(string path, string content)> entries)
    {
        var zipPath = Path.Combine(_tempDir, $"{Guid.NewGuid()}.zip");
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var (path, content) in entries)
        {
            var entry = archive.CreateEntry(path);
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream);
            writer.Write(content);
        }
        return zipPath;
    }
}
