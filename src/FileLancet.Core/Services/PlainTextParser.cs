using FileLancet.Core.Models;

namespace FileLancet.Core.Services;

/// <summary>
/// 纯文本解析器（用于验证扩展性）
/// </summary>
public class PlainTextParser : BaseParser
{
    public PlainTextParser(ILogger<BaseParser>? logger = null) : base(logger)
    {
    }

    /// <inheritdoc />
    public override bool CanParse(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension == ".txt" || extension == ".log" || extension == ".md";
    }

    /// <inheritdoc />
    public override ParseResult Parse(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new ParseResult
                {
                    Success = false,
                    ErrorMessage = "文件不存在",
                    SourcePath = filePath
                };
            }

            // 读取文件内容
            var content = File.ReadAllText(filePath);
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // 创建根节点
            var root = CreateNode(Path.GetFileName(filePath), "", NodeType.Root);

            // 添加内容节点
            var contentNode = CreateNode("content", "content", NodeType.Other);
            contentNode.Size = content.Length;
            contentNode.MimeType = GetMimeType(filePath);
            root.Children.Add(contentNode);

            // 添加行节点（最多显示前100行）
            int lineCount = Math.Min(lines.Length, 100);
            for (int i = 0; i < lineCount; i++)
            {
                var lineNode = CreateNode($"Line {i + 1}", $"line{i}", NodeType.Other);
                lineNode.Description = lines[i].Length > 50 
                    ? lines[i].Substring(0, 50) + "..." 
                    : lines[i];
                contentNode.Children.Add(lineNode);
            }

            // 如果有更多行，添加省略节点
            if (lines.Length > 100)
            {
                var moreNode = CreateNode($"... 还有 {lines.Length - 100} 行", "more", NodeType.Other);
                contentNode.Children.Add(moreNode);
            }

            // 创建文件详情
            var details = CreateFileDetails(filePath);
            details.Title = Path.GetFileNameWithoutExtension(filePath);
            details.Authors = new List<string> { "Unknown" };
            
            // 尝试从内容中提取标题（第一行）
            if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
            {
                details.Title = lines[0].Trim();
            }

            return new ParseResult
            {
                Success = true,
                RootNode = root,
                Details = details
            };
        }
        catch (Exception ex)
        {
            return HandleException(ex, filePath);
        }
    }

    /// <inheritdoc />
    protected override string GetNodeDescription(NodeType type)
    {
        return type switch
        {
            NodeType.Root => "文本文件",
            NodeType.Other => "文本内容",
            _ => "未知节点"
        };
    }
}
