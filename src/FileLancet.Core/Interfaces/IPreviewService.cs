using FileLancet.Core.Models;

namespace FileLancet.Core.Interfaces;

/// <summary>
/// 预览内容类型
/// </summary>
public enum PreviewContentType
{
    None,
    Text,
    Html,
    Image,
    Binary,
    Code
}

/// <summary>
/// 预览结果
/// </summary>
public class PreviewResult
{
    /// <summary>
    /// 预览类型
    /// </summary>
    public PreviewContentType ContentType { get; set; }

    /// <summary>
    /// 文本内容（用于文本、HTML、代码预览）
    /// </summary>
    public string TextContent { get; set; } = "";

    /// <summary>
    /// 图片数据（用于图片预览）
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// 图片格式
    /// </summary>
    public string ImageFormat { get; set; } = "";

    /// <summary>
    /// 二进制文件信息
    /// </summary>
    public string BinaryInfo { get; set; } = "";

    /// <summary>
    /// 代码语言类型（用于语法高亮）
    /// </summary>
    public string CodeLanguage { get; set; } = "";

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; } = "";

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 创建文本预览结果
    /// </summary>
    public static PreviewResult Text(string content, string language = "")
    {
        return new PreviewResult
        {
            ContentType = string.IsNullOrEmpty(language) ? PreviewContentType.Text : PreviewContentType.Code,
            TextContent = content,
            CodeLanguage = language
        };
    }

    /// <summary>
    /// 创建 HTML 预览结果
    /// </summary>
    public static PreviewResult Html(string htmlContent)
    {
        return new PreviewResult
        {
            ContentType = PreviewContentType.Html,
            TextContent = htmlContent
        };
    }

    /// <summary>
    /// 创建图片预览结果
    /// </summary>
    public static PreviewResult Image(byte[] data, string format)
    {
        return new PreviewResult
        {
            ContentType = PreviewContentType.Image,
            ImageData = data,
            ImageFormat = format
        };
    }

    /// <summary>
    /// 创建二进制预览结果
    /// </summary>
    public static PreviewResult Binary(string info, long size)
    {
        return new PreviewResult
        {
            ContentType = PreviewContentType.Binary,
            BinaryInfo = info,
            FileSize = size
        };
    }

    /// <summary>
    /// 创建错误结果
    /// </summary>
    public static PreviewResult Error(string message)
    {
        return new PreviewResult
        {
            Success = false,
            ErrorMessage = message,
            ContentType = PreviewContentType.None
        };
    }
}

/// <summary>
/// 预览服务接口
/// </summary>
public interface IPreviewService
{
    /// <summary>
    /// 根据节点类型获取预览内容
    /// </summary>
    Task<PreviewResult> GetPreviewAsync(FileNode node, IContentLoader contentLoader);

    /// <summary>
    /// 获取预览服务的支持类型
    /// </summary>
    IEnumerable<NodeType> SupportedTypes { get; }
}
