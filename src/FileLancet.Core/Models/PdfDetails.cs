namespace FileLancet.Core.Models;

/// <summary>
/// PDF 文件详细信息
/// </summary>
public class PdfDetails : FileDetails
{
    /// <summary>
    /// PDF 版本（如 "1.4", "1.7"）
    /// </summary>
    public string? PdfVersion { get; set; }
    
    /// <summary>
    /// 页面数量
    /// </summary>
    public int PageCount { get; set; }
    
    /// <summary>
    /// 是否加密
    /// </summary>
    public bool IsEncrypted { get; set; }
    
    /// <summary>
    /// 创建工具
    /// </summary>
    public string? Creator { get; set; }
    
    /// <summary>
    /// PDF 生成器
    /// </summary>
    public string? Producer { get; set; }
    
    /// <summary>
    /// 主题
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// 关键词
    /// </summary>
    public string? Keywords { get; set; }
    
    /// <summary>
    /// 页面信息列表
    /// </summary>
    public List<PdfPageInfo> Pages { get; set; } = new();
}
