using System.Windows;
using System.Windows.Controls;
using FileLancet.Core.Models;
using FileLancet.UI.ViewModels;

namespace FileLancet.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupDragDrop();
        }

        /// <summary>
        /// 设置拖拽支持
        /// </summary>
        private void SetupDragDrop()
        {
            AllowDrop = true;

            PreviewDragOver += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                }
            };

            Drop += async (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0 && DataContext is MainViewModel viewModel)
                    {
                        // 清除当前选中节点，避免旧文件状态干扰
                        viewModel.SelectedNode = null;
                        
                        // 加载新文件
                        await viewModel.LoadFileAsync(files[0]);

                        // 如果有多个文件，显示提示
                        if (files.Length > 1)
                        {
                            viewModel.StatusMessage = $"Loaded: {System.IO.Path.GetFileName(files[0])} (and {files.Length - 1} more files ignored)";
                        }
                    }
                }
                e.Handled = true;
            };
        }

        /// <summary>
        /// TreeView selection changed handler
        /// </summary>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel viewModel && e.NewValue is FileNode node)
            {
                viewModel.SelectedNode = node;
            }
        }

        /// <summary>
        /// Exit menu item click handler
        /// </summary>
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// About menu item click handler
        /// </summary>
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "File Lancet - File Analyzer\n\n" +
                "Version v0.2.0\n" +
                "A tool for analyzing file structure\n\n" +
                "Features:\n" +
                "- Multi-format support (EPUB, PDF, TXT, etc.)\n" +
                "- PDF Structure Analysis\n" +
                "- Content Preview\n" +
                "- Drag & Drop Support\n" +
                "- Syntax Highlighting\n" +
                "- Structured Code View",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

    }
}
