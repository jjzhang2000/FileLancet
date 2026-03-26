using System.IO;
using System.Windows;
using FileLancet.UI.ViewModels;
using FileLancet.UI.Views;

namespace FileLancet.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 启动时传递的文件路径（用于文件关联）
    /// </summary>
    public static string? StartupFilePath { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 处理命令行参数（文件关联支持）
        if (e.Args.Length > 0)
        {
            StartupFilePath = e.Args[0];
        }

        // 创建主窗口
        var mainWindow = new MainWindow();

        // 如果有启动文件路径，自动加载
        if (!string.IsNullOrEmpty(StartupFilePath) && File.Exists(StartupFilePath))
        {
            if (mainWindow.DataContext is MainViewModel viewModel)
            {
                _ = viewModel.LoadFileAsync(StartupFilePath);
            }
        }

        mainWindow.Show();
    }
}
