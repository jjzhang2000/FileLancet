using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileLancet.Core.Models;
using FileLancet.Core.Services;

namespace FileLancet.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ParserFactory _parserFactory;

    [ObservableProperty]
    private ObservableCollection<FileNode> _fileTreeNodes = new();

    [ObservableProperty]
    private FileNode? _selectedNode;

    [ObservableProperty]
    private FileDetails? _nodeDetails;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    [ObservableProperty]
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public MainViewModel()
    {
        var parsers = new List<IFileLancetParser> { new EpubParser() };
        _parserFactory = new ParserFactory(parsers);
    }

    partial void OnSelectedNodeChanged(FileNode? value)
    {
        if (value != null)
        {
            StatusMessage = value.Description;
        }
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "打开 EPUB 文件",
            AllowMultiple = false
        };

        dialog.Filters.Add(new FileDialogFilter
        {
            Name = "EPUB 文件",
            Extensions = { "epub" }
        });

        var window = App.Current?.ApplicationLifetime 
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
            ? desktop.MainWindow : null;

        if (window == null) return;

        var result = await dialog.ShowAsync(window);
        if (result?.Length > 0)
        {
            await LoadFileAsync(result[0]);
        }
    }

    [RelayCommand]
    private async Task LoadFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        IsLoading = true;
        StatusMessage = "正在解析文件...";

        try
        {
            var parser = _parserFactory.GetParser(filePath);
            if (parser == null)
            {
                StatusMessage = "不支持的文件格式";
                return;
            }

            var result = await parser.ParseAsync(filePath);

            if (result.Success)
            {
                CurrentFilePath = filePath;
                FileTreeNodes.Clear();
                if (result.RootNode != null)
                {
                    FileTreeNodes.Add(result.RootNode);
                }
                NodeDetails = result.Details;
                StatusMessage = $"解析完成，耗时: {result.ParseTime.TotalMilliseconds:F0}ms";
            }
            else
            {
                StatusMessage = $"解析失败: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"错误: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
