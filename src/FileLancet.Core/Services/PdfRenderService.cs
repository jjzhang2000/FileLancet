using FileLancet.Core.Interfaces;
using UglyToad.PdfPig;
using SkiaSharp;

namespace FileLancet.Core.Services;

/// <summary>
/// PDF 渲染服务实现
/// </summary>
public class PdfRenderService : IPdfRenderService
{
    /// <inheritdoc />
    public async Task<byte[]?> RenderPageAsync(string pdfPath, int pageNumber,
        double scale = 1.0, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var document = PdfDocument.Open(pdfPath);
            var page = document.GetPage(pageNumber);

            // 计算渲染尺寸（2x for better quality）
            int width = (int)(page.Width * scale * 2);
            int height = (int)(page.Height * scale * 2);

            // 创建 SkiaSharp 位图
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);

            // 白色背景
            canvas.Clear(SKColors.White);

            // 渲染 PDF 页面内容
            // 注意：PdfPig 本身不提供渲染功能，这里使用文本占位符表示
            // 实际渲染需要配合其他库（如 Pdfium）或自定义渲染逻辑
            var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 24 * (float)scale,
                IsAntialias = true
            };

            // 绘制页码
            canvas.DrawText($"Page {pageNumber}", 20, 40, paint);

            // 绘制页面尺寸信息
            paint.TextSize = 16 * (float)scale;
            canvas.DrawText($"{page.Width:F0} x {page.Height:F0} pts", 20, 70, paint);

            // 绘制文本内容预览（前500字符）
            var text = page.Text;
            if (!string.IsNullOrEmpty(text))
            {
                var previewText = text.Length > 500 ? text[..500] + "..." : text;
                var lines = previewText.Split('\n').Take(20).ToArray();
                float y = 120;
                foreach (var line in lines)
                {
                    if (y > height - 20) break;
                    var truncatedLine = line.Length > 80 ? line[..80] : line;
                    canvas.DrawText(truncatedLine, 20, y, paint);
                    y += 25 * (float)scale;
                }
            }

            canvas.Flush();

            // 编码为 PNG 字节数组
            return EncodeToPng(bitmap);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<byte[]?> RenderThumbnailAsync(string pdfPath, int pageNumber,
        int maxWidth = 200, CancellationToken cancellationToken = default)
    {
        var (pageWidth, _) = await GetPageSizeAsync(pdfPath, pageNumber);

        // 计算缩放比例
        double scale = maxWidth / pageWidth;

        return await RenderPageAsync(pdfPath, pageNumber, scale, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> ExtractPageTextAsync(string pdfPath, int pageNumber,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var document = PdfDocument.Open(pdfPath);
            var page = document.GetPage(pageNumber);
            return page.Text ?? string.Empty;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(double Width, double Height)> GetPageSizeAsync(string pdfPath, int pageNumber)
    {
        return await Task.Run(() =>
        {
            using var document = PdfDocument.Open(pdfPath);
            var page = document.GetPage(pageNumber);
            return (page.Width, page.Height);
        });
    }

    /// <summary>
    /// 将 SKBitmap 编码为 PNG 字节数组
    /// </summary>
    private static byte[] EncodeToPng(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
