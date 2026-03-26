using System.IO.Compression;
using System.Text;
using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;
using FileLancet.Core.Services;

namespace FileLancet.Core.Tests.Services;

/// <summary>
/// 内容加载器测试 - TC-301 ~ TC-307
/// </summary>
public class ContentLoaderTests : IDisposable
{
    private readonly string _testEpubPath;
    private readonly string _tempDir;

    public ContentLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _testEpubPath = Path.Combine(_tempDir, "test.epub");
        CreateTestEpub(_testEpubPath);
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

    private void CreateTestEpub(string path)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);

        // mimetype
        var mimetypeEntry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
        using (var stream = mimetypeEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("application/epub+zip");
        }

        // container.xml
        var containerEntry = archive.CreateEntry("META-INF/container.xml");
        using (var stream = containerEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("""<?xml version="1.0"?><container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container"><rootfiles><rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/></rootfiles></container>""");
        }

        // content.opf
        var opfEntry = archive.CreateEntry("OEBPS/content.opf");
        using (var stream = opfEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("""<?xml version="1.0"?><package version="3.0" xmlns="http://www.idpf.org/2007/opf"><metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>Test</dc:title></metadata><manifest><item id="html1" href="chapter1.xhtml" media-type="application/xhtml+xml"/></manifest><spine><itemref idref="html1"/></spine></package>""");
        }

        // chapter1.xhtml
        var htmlEntry = archive.CreateEntry("OEBPS/chapter1.xhtml");
        using (var stream = htmlEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("<html><body><h1>Test Chapter</h1><p>Hello World</p></body></html>");
        }

        // style.css
        var cssEntry = archive.CreateEntry("OEBPS/style.css");
        using (var stream = cssEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("body { color: black; }");
        }

        // script.js
        var jsEntry = archive.CreateEntry("OEBPS/script.js");
        using (var stream = jsEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("function test() { return true; }");
        }

        // test.png (模拟图片)
        var imgEntry = archive.CreateEntry("OEBPS/test.png");
        using (var stream = imgEntry.Open())
        {
            // PNG 文件头
            var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            stream.Write(pngHeader, 0, pngHeader.Length);
        }
    }

    [Fact(DisplayName = "TC-301: PreviewService 初始化")]
    public void PreviewService_Initialization_ShouldCreateInstance()
    {
        var service = new PreviewService();

        Assert.NotNull(service);
        Assert.NotEmpty(service.SupportedTypes);
    }

    [Fact(DisplayName = "TC-302: HTML 预览生成")]
    public async Task PreviewService_HtmlNode_ShouldReturnHtmlPreview()
    {
        var service = new PreviewService();
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "chapter1.xhtml",
            Path = "OEBPS/chapter1.xhtml",
            Type = NodeType.Html,
            Size = 100
        };

        var result = await service.GetPreviewAsync(node, loader);

        Assert.True(result.Success);
        Assert.Equal(PreviewContentType.Html, result.ContentType);
        Assert.Contains("Test Chapter", result.TextContent);
    }

    [Fact(DisplayName = "TC-303: 图片预览生成")]
    public async Task PreviewService_ImageNode_ShouldReturnImagePreview()
    {
        var service = new PreviewService();
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "test.png",
            Path = "OEBPS/test.png",
            Type = NodeType.Image,
            Size = 8
        };

        var result = await service.GetPreviewAsync(node, loader);

        Assert.True(result.Success);
        Assert.Equal(PreviewContentType.Image, result.ContentType);
        Assert.NotNull(result.ImageData);
        Assert.Equal("png", result.ImageFormat);
    }

    [Fact(DisplayName = "TC-304: 文本预览生成")]
    public async Task PreviewService_CssNode_ShouldReturnTextPreview()
    {
        var service = new PreviewService();
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "style.css",
            Path = "OEBPS/style.css",
            Type = NodeType.Css,
            Size = 100
        };

        var result = await service.GetPreviewAsync(node, loader);

        Assert.True(result.Success);
        Assert.Equal(PreviewContentType.Code, result.ContentType);
        Assert.Equal("css", result.CodeLanguage);
        Assert.Contains("color: black", result.TextContent);
    }

    [Fact(DisplayName = "TC-305: 二进制预览生成")]
    public async Task PreviewService_FontNode_ShouldReturnBinaryPreview()
    {
        var service = new PreviewService();
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "font.ttf",
            Path = "OEBPS/font.ttf",
            Type = NodeType.Font,
            Size = 1024
        };

        var result = await service.GetPreviewAsync(node, loader);

        Assert.True(result.Success);
        Assert.Equal(PreviewContentType.Binary, result.ContentType);
        Assert.Contains("Font", result.BinaryInfo);
    }

    [Fact(DisplayName = "TC-306: 预览内容加载 - 大图片处理")]
    public async Task PreviewService_LargeImage_ShouldHandleGracefully()
    {
        var service = new PreviewService();
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "test.png",
            Path = "OEBPS/test.png",
            Type = NodeType.Image,
            Size = 8
        };

        var result = await service.GetPreviewAsync(node, loader);

        Assert.True(result.Success);
        Assert.NotNull(result.ImageData);
    }

    [Fact(DisplayName = "TC-307: 预览缓存")]
    public async Task ContentLoader_Cache_ShouldReturnCachedContent()
    {
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "chapter1.xhtml",
            Path = "OEBPS/chapter1.xhtml",
            Type = NodeType.Html
        };

        // 第一次加载
        var content1 = await loader.LoadContentAsync(node);
        // 第二次加载（应该从缓存）
        var content2 = await loader.LoadContentAsync(node);

        Assert.Equal(content1, content2);
        Assert.True(loader.TryGetFromCache(node.Path, out _));
    }

    [Fact(DisplayName = "TC-308: 文件夹预览")]
    public async Task PreviewService_FolderNode_ShouldReturnFolderPreview()
    {
        var service = new PreviewService();
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "OEBPS",
            Path = "OEBPS",
            Type = NodeType.Folder,
            Children =
            {
                new FileNode { Name = "test1.html", Type = NodeType.Html },
                new FileNode { Name = "test2.css", Type = NodeType.Css }
            }
        };

        var result = await service.GetPreviewAsync(node, loader);

        Assert.True(result.Success);
        Assert.Equal(PreviewContentType.Text, result.ContentType);
        Assert.Contains("OEBPS", result.TextContent);
        Assert.Contains("2", result.TextContent);
    }

    [Fact(DisplayName = "TC-309: JavaScript 预览")]
    public async Task PreviewService_JsNode_ShouldReturnCodePreview()
    {
        var service = new PreviewService();
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "script.js",
            Path = "OEBPS/script.js",
            Type = NodeType.Script,
            Size = 100
        };

        var result = await service.GetPreviewAsync(node, loader);

        Assert.True(result.Success);
        Assert.Equal(PreviewContentType.Code, result.ContentType);
        Assert.Equal("javascript", result.CodeLanguage);
    }

    [Fact(DisplayName = "TC-310: XML 预览")]
    public async Task PreviewService_XmlNode_ShouldReturnCodePreview()
    {
        var service = new PreviewService();
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "container.xml",
            Path = "META-INF/container.xml",
            Type = NodeType.Container,
            Size = 100
        };

        var result = await service.GetPreviewAsync(node, loader);

        Assert.True(result.Success);
        Assert.Equal(PreviewContentType.Code, result.ContentType);
        Assert.Equal("xml", result.CodeLanguage);
    }

    [Fact(DisplayName = "TC-311: 空节点处理")]
    public async Task PreviewService_NullNode_ShouldReturnError()
    {
        var service = new PreviewService();
        var loader = new EpubContentLoader(_testEpubPath);

        var result = await service.GetPreviewAsync(null!, loader);

        Assert.False(result.Success);
        Assert.Contains("节点为空", result.ErrorMessage);
    }

    [Fact(DisplayName = "TC-312: 空加载器处理")]
    public async Task PreviewService_NullLoader_ShouldReturnError()
    {
        var service = new PreviewService();
        var node = new FileNode { Name = "test.html", Type = NodeType.Html };

        var result = await service.GetPreviewAsync(node, null!);

        Assert.False(result.Success);
        Assert.Contains("内容加载器为空", result.ErrorMessage);
    }

    [Fact(DisplayName = "TC-313: 内容加载器编码检测")]
    public async Task ContentLoader_LoadText_ShouldDetectEncoding()
    {
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "chapter1.xhtml",
            Path = "OEBPS/chapter1.xhtml",
            Type = NodeType.Html
        };

        var text = await loader.LoadTextAsync(node);

        Assert.NotNull(text);
        Assert.Contains("Test Chapter", text);
    }

    [Fact(DisplayName = "TC-314: 内容存在检查")]
    public void ContentLoader_ContentExists_ShouldReturnCorrectResult()
    {
        var loader = new EpubContentLoader(_testEpubPath);
        var existingNode = new FileNode
        {
            Name = "chapter1.xhtml",
            Path = "OEBPS/chapter1.xhtml",
            Type = NodeType.Html
        };
        var nonExistingNode = new FileNode
        {
            Name = "nonexistent.html",
            Path = "OEBPS/nonexistent.html",
            Type = NodeType.Html
        };

        Assert.True(loader.ContentExists(existingNode));
        Assert.False(loader.ContentExists(nonExistingNode));
    }

    [Fact(DisplayName = "TC-315: 缓存清除")]
    public async Task ContentLoader_ClearCache_ShouldRemoveCachedItems()
    {
        var loader = new EpubContentLoader(_testEpubPath);
        var node = new FileNode
        {
            Name = "chapter1.xhtml",
            Path = "OEBPS/chapter1.xhtml",
            Type = NodeType.Html
        };

        // 先加载以填充缓存
        await loader.LoadContentAsync(node);
        Assert.True(loader.TryGetFromCache(node.Path, out _));

        // 清除缓存
        loader.ClearCache();
        Assert.False(loader.TryGetFromCache(node.Path, out _));
    }

    [Fact(DisplayName = "TC-316: 预览服务支持类型")]
    public void PreviewService_SupportedTypes_ShouldIncludeExpectedTypes()
    {
        var service = new PreviewService();

        var supportedTypes = service.SupportedTypes.ToList();

        Assert.Contains(NodeType.Html, supportedTypes);
        Assert.Contains(NodeType.Css, supportedTypes);
        Assert.Contains(NodeType.Image, supportedTypes);
        Assert.Contains(NodeType.Script, supportedTypes);
    }
}
