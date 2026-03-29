namespace FileLancet.Core.Models;

/// <summary>
/// PDF 页面信息
/// </summary>
public class PdfPageInfo
{
    /// <summary>
    /// 页码（从1开始）
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// 页面宽度（点）
    /// </summary>
    public double Width { get; set; }
    
    /// <summary>
    /// 页面高度（点）
    /// </summary>
    public double Height { get; set; }
    
    /// <summary>
    /// 旋转角度（0, 90, 180, 270）
    /// </summary>
    public int Rotation { get; set; }
    
    /// <summary>
    /// 媒体框信息
    /// </summary>
    public string? MediaBox { get; set; }
}
