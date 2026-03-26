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
                "File Lancet - EPUB Analyzer\n\n" +
                "Version 0.1\n" +
                "A tool for analyzing EPUB file structure",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
