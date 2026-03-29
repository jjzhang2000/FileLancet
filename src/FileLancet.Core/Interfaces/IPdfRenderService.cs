namespace FileLancet.Core.Interfaces;

/// <summary>
/// PDF 渲染服务接口
/// </summary>
public interface IPdfRenderService
{
    /// <summary>
    /// 渲染指定页面为 PNG 图像字节数组
    /// </summary>
    /// <param name="pdfPath">PDF 文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="scale">缩放比例（1.0 = 实际大小）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>PNG 图像字节数组</returns>
    Task<byte[]?> RenderPageAsync(string pdfPath, int pageNumber, 
        double scale = 1.0, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 渲染页面缩略图为 PNG 图像字节数组
    /// </summary>
    /// <param name="pdfPath">PDF 文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="maxWidth">最大宽度</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>PNG 图像字节数组</returns>
    Task<byte[]?> RenderThumbnailAsync(string pdfPath, int pageNumber, 
        int maxWidth = 200, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 提取页面文本内容
    /// </summary>
    /// <param name="pdfPath">PDF 文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>页面文本内容</returns>
    Task<string> ExtractPageTextAsync(string pdfPath, int pageNumber, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取页面尺寸
    /// </summary>
    /// <param name="pdfPath">PDF 文件路径</param>
    /// <param name="pageNumber">页码（从1开始）</param>
    /// <returns>页面宽度和高度（点）</returns>
    Task<(double Width, double Height)> GetPageSizeAsync(string pdfPath, int pageNumber);
}
