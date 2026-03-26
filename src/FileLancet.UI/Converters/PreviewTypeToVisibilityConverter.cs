using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FileLancet.UI.ViewModels;

namespace FileLancet.UI.Converters
{
    /// <summary>
    /// PreviewType to Visibility converter
    /// </summary>
    public class PreviewTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PreviewType previewType && parameter is string targetTypeStr)
        {
            return previewType.ToString() == targetTypeStr ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
