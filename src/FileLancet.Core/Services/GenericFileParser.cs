using FileLancet.Core.Models;

namespace FileLancet.Core.Services;

/// <summary>
/// 通用文件解析器 - 作为兜底解析器，支持任何文件格式
/// </summary>
public class GenericFileParser : BaseParser
{
    public GenericFileParser(ILogger<BaseParser>? logger = null) : base(logger)
    {
    }

    /// <inheritdoc />
    public override bool CanParse(string filePath)
    {
        // 通用解析器可以解析任何存在的文件
        return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
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

            var fileInfo = new FileInfo(filePath);
            
            // 创建根节点 - 只保留文件名
            var root = CreateNode(fileInfo.Name, "", NodeType.Root);
            
            // 创建文件详情 - 显示基本信息
            var details = new FileDetails
            {
                Title = fileInfo.Name,
                FilePath = fileInfo.FullName,
                FileSize = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime,
                CreatedTime = fileInfo.CreationTime,
                FileExtension = fileInfo.Extension,
                MimeType = GetMimeType(fileInfo.Name)
            };

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
            NodeType.Root => "文件",
            _ => "未知节点"
        };
    }
}
