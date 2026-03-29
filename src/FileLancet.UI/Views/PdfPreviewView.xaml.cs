using System.Windows.Controls;
using FileLancet.UI.ViewModels;

namespace FileLancet.UI.Views;

/// <summary>
/// PdfPreviewView.xaml 的交互逻辑
/// </summary>
public partial class PdfPreviewView : UserControl
{
    public PdfPreviewView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 构造函数，带视图模型
    /// </summary>
    public PdfPreviewView(PdfPreviewViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
