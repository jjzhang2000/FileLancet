using System.Text;
using System.Text.RegularExpressions;
using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;

namespace FileLancet.Core.Services;

/// <summary>
/// 预览服务实现
/// </summary>
public class PreviewService : IPreviewService
{
    private readonly Dictionary<NodeType, Func<FileNode, IContentLoader, Task<PreviewResult>>> _previewHandlers;

    public PreviewService()
    {
        _previewHandlers = new Dictionary<NodeType, Func<FileNode, IContentLoader, Task<PreviewResult>>>
        {
            [NodeType.Html] = PreviewHtmlAsync,
            [NodeType.Css] = PreviewCssAsync,
            [NodeType.Script] = PreviewScriptAsync,
            [NodeType.Image] = PreviewImageAsync,
            [NodeType.Font] = PreviewBinaryAsync,
            [NodeType.Audio] = PreviewBinaryAsync,
            [NodeType.Video] = PreviewBinaryAsync,
            [NodeType.Other] = PreviewBinaryAsync,
            [NodeType.Container] = PreviewXmlAsync,
            [NodeType.Opf] = PreviewXmlAsync,
            [NodeType.Ncx] = PreviewXmlAsync,
            [NodeType.Nav] = PreviewHtmlAsync
        };
    }

    /// <inheritdoc />
    public IEnumerable<NodeType> SupportedTypes => _previewHandlers.Keys;

    /// <inheritdoc />
    public async Task<PreviewResult> GetPreviewAsync(FileNode node, IContentLoader contentLoader)
    {
        if (node == null)
            return PreviewResult.Error("节点为空");

        if (contentLoader == null)
            return PreviewResult.Error("内容加载器为空");

        try
        {
            // 文件夹和根节点特殊处理
            if (node.Type == NodeType.Folder || node.Type == NodeType.Root)
            {
                return PreviewFolder(node);
            }

            // 查找对应的预览处理器
            if (_previewHandlers.TryGetValue(node.Type, out var handler))
            {
                return await handler(node, contentLoader);
            }

            // 默认使用二进制预览
            return PreviewBinary(node);
        }
        catch (Exception ex)
        {
            return PreviewResult.Error($"预览生成失败: {ex.Message}");
        }
    }

    /// <summary>
    /// HTML 预览
    /// </summary>
    private async Task<PreviewResult> PreviewHtmlAsync(FileNode node, IContentLoader contentLoader)
    {
        var content = await contentLoader.LoadTextAsync(node);
        return PreviewResult.Html(content);
    }

    /// <summary>
    /// CSS 预览（带语法高亮标记）
    /// </summary>
    private async Task<PreviewResult> PreviewCssAsync(FileNode node, IContentLoader contentLoader)
    {
        var content = await contentLoader.LoadTextAsync(node);
        return PreviewResult.Text(content, "css");
    }

    /// <summary>
    /// JavaScript 预览
    /// </summary>
    private async Task<PreviewResult> PreviewScriptAsync(FileNode node, IContentLoader contentLoader)
    {
        var content = await contentLoader.LoadTextAsync(node);
        return PreviewResult.Text(content, "javascript");
    }

    /// <summary>
    /// XML 预览
    /// </summary>
    private async Task<PreviewResult> PreviewXmlAsync(FileNode node, IContentLoader contentLoader)
    {
        var content = await contentLoader.LoadTextAsync(node);
        return PreviewResult.Text(content, "xml");
    }

    /// <summary>
    /// 图片预览
    /// </summary>
    private async Task<PreviewResult> PreviewImageAsync(FileNode node, IContentLoader contentLoader)
    {
        var content = await contentLoader.LoadContentAsync(node);
        var format = GetImageFormat(node.Name);
        return PreviewResult.Image(content, format);
    }

    /// <summary>
    /// 二进制文件预览
    /// </summary>
    private Task<PreviewResult> PreviewBinaryAsync(FileNode node, IContentLoader contentLoader)
    {
        return Task.FromResult(PreviewBinary(node));
    }

    /// <summary>
    /// 文件夹预览
    /// </summary>
    private PreviewResult PreviewFolder(FileNode node)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"文件夹: {node.Name}");
        sb.AppendLine($"路径: {node.Path}");
        sb.AppendLine($"包含项目: {node.Children.Count}");

        if (node.Children.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("内容列表:");
            foreach (var child in node.Children.Take(20))
            {
                var typeIcon = child.Type switch
                {
                    NodeType.Folder => "[文件夹]",
                    NodeType.Html => "[HTML]",
                    NodeType.Image => "[图片]",
                    NodeType.Css => "[CSS]",
                    _ => "[文件]"
                };
                sb.AppendLine($"  {typeIcon} {child.Name}");
            }

            if (node.Children.Count > 20)
            {
                sb.AppendLine($"  ... 还有 {node.Children.Count - 20} 个项目");
            }
        }

        return PreviewResult.Text(sb.ToString());
    }

    /// <summary>
    /// 二进制文件预览
    /// </summary>
    private PreviewResult PreviewBinary(FileNode node)
    {
        var info = $"{node.Type} 文件\n" +
                   $"文件名: {node.Name}\n" +
                   $"大小: {FormatFileSize(node.Size)}\n" +
                   $"MIME 类型: {node.MimeType ?? "未知"}";

        return PreviewResult.Binary(info, node.Size);
    }

    /// <summary>
    /// 获取图片格式
    /// </summary>
    private static string GetImageFormat(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "jpeg",
            ".png" => "png",
            ".gif" => "gif",
            ".bmp" => "bmp",
            ".webp" => "webp",
            ".svg" => "svg",
            _ => "unknown"
        };
    }

    /// <summary>
    /// 格式化文件大小
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F2} MB";
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}

/// <summary>
/// 语法高亮服务
/// </summary>
public class SyntaxHighlighter
{
    /// <summary>
    /// 对代码进行语法高亮处理
    /// </summary>
    public string Highlight(string code, string language)
    {
        if (string.IsNullOrEmpty(code))
            return code;

        return language.ToLowerInvariant() switch
        {
            "css" => HighlightCss(code),
            "javascript" or "js" => HighlightJavaScript(code),
            "xml" or "html" => HighlightXml(code),
            _ => EscapeHtml(code)
        };
    }

    /// <summary>
    /// CSS 语法高亮
    /// </summary>
    private string HighlightCss(string code)
    {
        // 简单的正则替换实现基础高亮
        var result = EscapeHtml(code);

        // 选择器高亮
        result = Regex.Replace(result, @"([a-zA-Z][a-zA-Z0-9]*)\s*\{",
            "<span class=\"selector\">$1</span> {");

        // 属性高亮
        result = Regex.Replace(result, @"([a-zA-Z-]+)\s*:",
            "<span class=\"property\">$1</span>:");

        // 值高亮
        result = Regex.Replace(result, @":\s*([^;]+);",
            ": <span class=\"value\">$1</span>;");

        // 注释高亮
        result = Regex.Replace(result, @"(/\*[\s\S]*?\*/)",
            "<span class=\"comment\">$1</span>");

        return result;
    }

    /// <summary>
    /// JavaScript 语法高亮
    /// </summary>
    private string HighlightJavaScript(string code)
    {
        var result = EscapeHtml(code);

        // 关键字
        var keywords = new[] { "var", "let", "const", "function", "return", "if", "else", "for", "while", "class", "import", "export" };
        foreach (var keyword in keywords)
        {
            result = Regex.Replace(result, $@"\b({keyword})\b",
                $"<span class=\"keyword\">$1</span>");
        }

        // 字符串 - 使用单引号
        result = Regex.Replace(result, "'([^']*)'",
            "<span class=\"string\">'$1'</span>");

        // 注释
        result = Regex.Replace(result, @"(//.*$)",
            "<span class=\"comment\">$1</span>", RegexOptions.Multiline);

        return result;
    }

    /// <summary>
    /// XML/HTML 语法高亮
    /// </summary>
    private string HighlightXml(string code)
    {
        var result = EscapeHtml(code);

        // 标签
        result = Regex.Replace(result, @"&lt;([a-zA-Z][a-zA-Z0-9]*)",
            "&lt;<span class=\"tag\">$1</span>");

        // 属性
        result = Regex.Replace(result, @"\s([a-zA-Z-]+)=",
            " <span class=\"attr\">$1</span>=");

        // 属性值
        result = Regex.Replace(result, "=\"([^\"]*)\"",
            "=\"<span class=\"attr-value\">$1</span>\"");

        // 注释
        result = Regex.Replace(result, @"(&lt;!--[\s\S]*?--&gt;)",
            "<span class=\"comment\">$1</span>");

        return result;
    }

    /// <summary>
    /// HTML 转义
    /// </summary>
    private string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
