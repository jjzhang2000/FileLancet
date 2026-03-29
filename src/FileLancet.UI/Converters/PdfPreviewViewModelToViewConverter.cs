using System.Globalization;
using System.Windows.Data;
using FileLancet.UI.ViewModels;
using FileLancet.UI.Views;

namespace FileLancet.UI.Converters;

/// <summary>
/// 将 PdfPreviewViewModel 转换为 PdfPreviewView 的转换器
/// </summary>
public class PdfPreviewViewModelToViewConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PdfPreviewViewModel viewModel)
        {
            return new PdfPreviewView(viewModel);
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
