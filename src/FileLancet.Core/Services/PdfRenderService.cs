using FileLancet.Core.Interfaces;
using System.IO;
using PDFtoImage;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace FileLancet.Core.Services;

public class PdfRenderService : IPdfRenderService
{
    public async Task<byte[]?> RenderPageAsync(string pdfPath, int pageNumber,
        double scale = 1.0, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            int dpi = (int)(72 * scale);

            byte[] pdfBytes = File.ReadAllBytes(pdfPath);
            string pdfBase64 = Convert.ToBase64String(pdfBytes);

            var tempPng = Path.Combine(Path.GetTempPath(), $"pdf_render_{Guid.NewGuid():N}.png");

            Conversion.SavePng(tempPng, pdfBase64, null, pageNumber - 1, dpi);

            var result = File.ReadAllBytes(tempPng);
            File.Delete(tempPng);

            return result;
        }, cancellationToken);
    }

    public async Task<byte[]?> RenderThumbnailAsync(string pdfPath, int pageNumber,
        int maxWidth = 200, CancellationToken cancellationToken = default)
    {
        var (pageWidth, _) = await GetPageSizeAsync(pdfPath, pageNumber);
        int dpi = (int)(72.0 * maxWidth / pageWidth);
        return await Task.Run(() =>
        {
            byte[] pdfBytes = File.ReadAllBytes(pdfPath);
            string pdfBase64 = Convert.ToBase64String(pdfBytes);

            var tempPng = Path.Combine(Path.GetTempPath(), $"pdf_thumb_{Guid.NewGuid():N}.png");

            Conversion.SavePng(tempPng, pdfBase64, null, pageNumber - 1, dpi);

            var result = File.ReadAllBytes(tempPng);
            File.Delete(tempPng);
            return result;
        }, cancellationToken);
    }

    public async Task<string> ExtractPageTextAsync(string pdfPath, int pageNumber,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var document = new PdfDocument(new PdfReader(pdfPath));
            var page = document.GetPage(pageNumber);
            return PdfTextExtractor.GetTextFromPage(page);
        }, cancellationToken);
    }

    public async Task<(double Width, double Height)> GetPageSizeAsync(string pdfPath, int pageNumber)
    {
        return await Task.Run(() =>
        {
            using var document = new PdfDocument(new PdfReader(pdfPath));
            var page = document.GetPage(pageNumber);
            var pageSize = page.GetPageSize();
            return (pageSize.GetWidth(), pageSize.GetHeight());
        });
    }
}