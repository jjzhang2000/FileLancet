using System.IO.Compression;
using System.Xml.Linq;

namespace FileLancet.Core.Utilities;

/// <summary>
/// XML 解析辅助工具类
/// 处理不规范的 XML（如未声明命名空间前缀）
/// </summary>
public static class XmlParserHelper
{
    /// <summary>
    /// 从 ZIP 条目加载 XML 文档，自动修复常见的命名空间问题
    /// </summary>
    public static async Task<XDocument> LoadFromZipEntryAsync(ZipArchiveEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var stream = entry.Open();
        return await LoadFromStreamAsync(stream, cancellationToken);
    }

    /// <summary>
    /// 从流加载 XML 文档，自动修复常见的命名空间问题
    /// </summary>
    public static async Task<XDocument> LoadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var reader = new StreamReader(stream);
        var xmlContent = await reader.ReadToEndAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return ParseWithNamespaceFix(xmlContent);
    }

    /// <summary>
    /// 解析 XML 字符串，自动修复常见的命名空间问题
    /// </summary>
    public static XDocument ParseWithNamespaceFix(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
            throw new ArgumentException("XML content is empty", nameof(xmlContent));

        // 修复未声明的 DC 命名空间前缀
        xmlContent = FixDcNamespace(xmlContent);

        // 修复未声明的 OPF 命名空间前缀
        xmlContent = FixOpfNamespace(xmlContent);

        // 修复未声明的 XHTML 命名空间前缀
        xmlContent = FixXhtmlNamespace(xmlContent);

        return XDocument.Parse(xmlContent);
    }

    /// <summary>
    /// 修复 DC 命名空间声明
    /// </summary>
    private static string FixDcNamespace(string xmlContent)
    {
        // 如果使用了 dc: 前缀但没有声明 xmlns:dc
        if (xmlContent.Contains("<dc:") && !xmlContent.Contains("xmlns:dc"))
        {
            // 找到根元素（跳过 XML 声明）
            var rootStart = FindRootElementStart(xmlContent);
            if (rootStart >= 0)
            {
                var rootEnd = FindTagEnd(xmlContent, rootStart);
                if (rootEnd > rootStart)
                {
                    var insertPos = rootEnd;
                    xmlContent = xmlContent.Insert(insertPos, " xmlns:dc=\"http://purl.org/dc/elements/1.1/\"");
                }
            }
        }
        return xmlContent;
    }

    /// <summary>
    /// 修复 OPF 命名空间声明
    /// </summary>
    private static string FixOpfNamespace(string xmlContent)
    {
        // 如果使用了 opf: 前缀但没有声明 xmlns:opf
        if (xmlContent.Contains("<opf:") && !xmlContent.Contains("xmlns:opf"))
        {
            var rootStart = FindRootElementStart(xmlContent);
            if (rootStart >= 0)
            {
                var rootEnd = FindTagEnd(xmlContent, rootStart);
                if (rootEnd > rootStart)
                {
                    var insertPos = rootEnd;
                    xmlContent = xmlContent.Insert(insertPos, " xmlns:opf=\"http://www.idpf.org/2007/opf\"");
                }
            }
        }
        return xmlContent;
    }

    /// <summary>
    /// 修复 XHTML 命名空间声明
    /// </summary>
    private static string FixXhtmlNamespace(string xmlContent)
    {
        // 如果使用了 html 标签但没有声明命名空间
        if ((xmlContent.Contains("<html") || xmlContent.Contains("<HTML")) 
            && !xmlContent.Contains("xmlns=") 
            && !xmlContent.Contains("xmlns:html"))
        {
            var htmlIndex = xmlContent.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
            if (htmlIndex >= 0)
            {
                var tagEnd = FindTagEnd(xmlContent, htmlIndex);
                if (tagEnd > htmlIndex)
                {
                    var insertPos = tagEnd;
                    xmlContent = xmlContent.Insert(insertPos, " xmlns=\"http://www.w3.org/1999/xhtml\"");
                }
            }
        }
        return xmlContent;
    }

    /// <summary>
    /// 找到根元素的开始位置（跳过 XML 声明）
    /// </summary>
    private static int FindRootElementStart(string xmlContent)
    {
        var index = 0;
        while (index < xmlContent.Length)
        {
            var ltIndex = xmlContent.IndexOf('<', index);
            if (ltIndex < 0) return -1;
            
            // 跳过 XML 声明 <?xml...?> 和注释 <!--...-->
            if (xmlContent.Substring(ltIndex).StartsWith("<?xml"))
            {
                index = xmlContent.IndexOf("?>", ltIndex);
                if (index < 0) return -1;
                index += 2;
            }
            else if (xmlContent.Substring(ltIndex).StartsWith("<!--"))
            {
                index = xmlContent.IndexOf("-->", ltIndex);
                if (index < 0) return -1;
                index += 3;
            }
            else
            {
                return ltIndex;
            }
        }
        return -1;
    }

    /// <summary>
    /// 找到标签的结束位置（第一个 > 或空格）
    /// </summary>
    private static int FindTagEnd(string xmlContent, int startIndex)
    {
        for (int i = startIndex + 1; i < xmlContent.Length; i++)
        {
            var c = xmlContent[i];
            if (c == '>' || c == ' ' || c == '\t' || c == '\n' || c == '\r')
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 从 ZIP 归档中获取条目（支持多种路径格式）
    /// </summary>
    public static ZipArchiveEntry? GetZipEntry(ZipArchive archive, string path)
    {
        // 尝试原始路径
        var entry = archive.GetEntry(path);
        if (entry != null) return entry;

        // 尝试正斜杠格式
        var normalizedPath = path.Replace("\\", "/");
        entry = archive.GetEntry(normalizedPath);
        if (entry != null) return entry;

        // 尝试反斜杠格式
        var backslashPath = path.Replace("/", "\\");
        entry = archive.GetEntry(backslashPath);
        if (entry != null) return entry;

        // 尝试不区分大小写的搜索
        var lowerPath = normalizedPath.ToLowerInvariant();
        entry = archive.Entries.FirstOrDefault(e => 
            e.FullName.ToLowerInvariant() == lowerPath);

        return entry;
    }

    /// <summary>
    /// 安全地获取元素的本地名称（忽略命名空间）
    /// </summary>
    public static IEnumerable<XElement> GetElementsByLocalName(XContainer container, string localName)
    {
        return container.Descendants().Where(e => e.Name.LocalName == localName);
    }

    /// <summary>
    /// 安全地获取第一个匹配本地名称的元素
    /// </summary>
    public static XElement? GetFirstElementByLocalName(XContainer container, string localName)
    {
        return container.Descendants().FirstOrDefault(e => e.Name.LocalName == localName);
    }

    /// <summary>
    /// 安全地获取元素的值（通过本地名称）
    /// </summary>
    public static string? GetElementValueByLocalName(XContainer container, string localName)
    {
        return GetFirstElementByLocalName(container, localName)?.Value;
    }
}
