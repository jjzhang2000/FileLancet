using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FileLancet.Core.Interfaces;

namespace FileLancet.UI.ViewModels;

/// <summary>
/// PDF 预览视图模型
/// </summary>
public class PdfPreviewViewModel : INotifyPropertyChanged
{
    private readonly IPdfRenderService _renderService;
    private string? _pdfPath;
    private int _currentPage = 1;
    private int _totalPages;
    private BitmapSource? _pageImage;
    private double _zoomLevel = 1.0;
    private bool _showTextMode;
    private string _textContent = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isLoading;
    private double _pageWidth;
    private double _pageHeight;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PdfPreviewViewModel(IPdfRenderService renderService)
    {
        _renderService = renderService;

        // 初始化命令
        PreviousPageCommand = new RelayCommand(ExecutePreviousPage, () => CanGoToPreviousPage);
        NextPageCommand = new RelayCommand(ExecuteNextPage, () => CanGoToNextPage);
        FirstPageCommand = new RelayCommand(ExecuteFirstPage, () => CanGoToFirstPage);
        LastPageCommand = new RelayCommand(ExecuteLastPage, () => CanGoToLastPage);
        ZoomInCommand = new RelayCommand(ExecuteZoomIn);
        ZoomOutCommand = new RelayCommand(ExecuteZoomOut);
        FitWidthCommand = new RelayCommand(ExecuteFitWidth);
        FitHeightCommand = new RelayCommand(ExecuteFitHeight);
        ActualSizeCommand = new RelayCommand(ExecuteActualSize);
    }

    #region Properties

    /// <summary>
    /// PDF 文件路径
    /// </summary>
    public string? PdfPath
    {
        get => _pdfPath;
        set
        {
            if (_pdfPath != value)
            {
                _pdfPath = value;
                OnPropertyChanged();
                _ = LoadDocumentAsync();
            }
        }
    }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged();
                if (value >= 1 && value <= TotalPages)
                {
                    if (ShowTextMode)
                        _ = LoadTextContentAsync();
                    else
                        _ = RenderCurrentPageAsync();
                }
                RefreshCommandStates();
            }
        }
    }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages
    {
        get => _totalPages;
        set
        {
            if (_totalPages != value)
            {
                _totalPages = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 当前页面图像
    /// </summary>
    public BitmapSource? PageImage
    {
        get => _pageImage;
        set
        {
            if (_pageImage != value)
            {
                _pageImage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 缩放级别
    /// </summary>
    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            if (_zoomLevel != value)
            {
                _zoomLevel = value;
                OnPropertyChanged();
                if (!ShowTextMode)
                    _ = RenderCurrentPageAsync();
            }
        }
    }

    /// <summary>
    /// 是否显示文本模式
    /// </summary>
    public bool ShowTextMode
    {
        get => _showTextMode;
        set
        {
            if (_showTextMode != value)
            {
                _showTextMode = value;
                OnPropertyChanged();
                if (value)
                    _ = LoadTextContentAsync();
                else
                    _ = RenderCurrentPageAsync();
            }
        }
    }

    /// <summary>
    /// 文本内容
    /// </summary>
    public string TextContent
    {
        get => _textContent;
        set
        {
            if (_textContent != value)
            {
                _textContent = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 是否正在加载
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
                RefreshCommandStates();
            }
        }
    }

    /// <summary>
    /// 页面宽度
    /// </summary>
    public double PageWidth
    {
        get => _pageWidth;
        set
        {
            if (_pageWidth != value)
            {
                _pageWidth = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 页面高度
    /// </summary>
    public double PageHeight
    {
        get => _pageHeight;
        set
        {
            if (_pageHeight != value)
            {
                _pageHeight = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// 上一页命令
    /// </summary>
    public ICommand PreviousPageCommand { get; }

    /// <summary>
    /// 下一页命令
    /// </summary>
    public ICommand NextPageCommand { get; }

    /// <summary>
    /// 首页命令
    /// </summary>
    public ICommand FirstPageCommand { get; }

    /// <summary>
    /// 末页命令
    /// </summary>
    public ICommand LastPageCommand { get; }

    /// <summary>
    /// 放大命令
    /// </summary>
    public ICommand ZoomInCommand { get; }

    /// <summary>
    /// 缩小命令
    /// </summary>
    public ICommand ZoomOutCommand { get; }

    /// <summary>
    /// 适应宽度命令
    /// </summary>
    public ICommand FitWidthCommand { get; }

    /// <summary>
    /// 适应高度命令
    /// </summary>
    public ICommand FitHeightCommand { get; }

    /// <summary>
    /// 实际大小命令
    /// </summary>
    public ICommand ActualSizeCommand { get; }

    #endregion

    #region Command Conditions

    /// <summary>
    /// 是否可以转到上一页
    /// </summary>
    public bool CanGoToPreviousPage => CurrentPage > 1 && !IsLoading;

    /// <summary>
    /// 是否可以转到下一页
    /// </summary>
    public bool CanGoToNextPage => CurrentPage < TotalPages && !IsLoading;

    /// <summary>
    /// 是否可以转到首页
    /// </summary>
    public bool CanGoToFirstPage => CurrentPage > 1 && !IsLoading;

    /// <summary>
    /// 是否可以转到末页
    /// </summary>
    public bool CanGoToLastPage => CurrentPage < TotalPages && !IsLoading;

    #endregion

    #region Methods

        /// <summary>
        /// 设置总页数（在设置 PdfPath 之前调用）
        /// </summary>
        public void SetTotalPages(int totalPages)
        {
            TotalPages = totalPages;
        }

        /// <summary>
        /// 加载 PDF 文档
        /// </summary>
    private async Task LoadDocumentAsync()
    {
        if (string.IsNullOrEmpty(_pdfPath))
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "正在加载 PDF...";

            // 获取页面尺寸信息
            var (width, height) = await _renderService.GetPageSizeAsync(_pdfPath, 1);
            PageWidth = width;
            PageHeight = height;

            // 如果 TotalPages 未设置，则设置为 1
            if (TotalPages <= 0)
            {
                TotalPages = 1;
            }

            if (ShowTextMode)
                await LoadTextContentAsync();
            else
                await RenderCurrentPageAsync();

            StatusMessage = $"已加载 PDF - {TotalPages} 页";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 渲染当前页面
    /// </summary>
    private async Task RenderCurrentPageAsync()
    {
        if (string.IsNullOrEmpty(_pdfPath) || CurrentPage < 1)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = $"正在渲染第 {CurrentPage} 页...";

            var imageBytes = await _renderService.RenderPageAsync(_pdfPath, CurrentPage, ZoomLevel);
            PageImage = ConvertBytesToBitmapSource(imageBytes);

            StatusMessage = $"第 {CurrentPage} / {TotalPages} 页 - 缩放: {ZoomLevel:P0}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"渲染失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 将 PNG 字节数组转换为 BitmapSource
    /// </summary>
    private static BitmapSource? ConvertBytesToBitmapSource(byte[]? imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return null;

        using var stream = new MemoryStream(imageBytes);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    /// <summary>
    /// 加载文本内容
    /// </summary>
    private async Task LoadTextContentAsync()
    {
        if (string.IsNullOrEmpty(_pdfPath) || CurrentPage < 1)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = $"正在提取第 {CurrentPage} 页文本...";

            var text = await _renderService.ExtractPageTextAsync(_pdfPath, CurrentPage);
            TextContent = text;

            StatusMessage = $"第 {CurrentPage} / {TotalPages} 页 - 文本模式";
        }
        catch (Exception ex)
        {
            StatusMessage = $"文本提取失败: {ex.Message}";
            TextContent = string.Empty;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 执行上一页
    /// </summary>
    private void ExecutePreviousPage()
    {
        if (CurrentPage > 1)
            CurrentPage--;
    }

    /// <summary>
    /// 执行下一页
    /// </summary>
    private void ExecuteNextPage()
    {
        if (CurrentPage < TotalPages)
            CurrentPage++;
    }

    /// <summary>
    /// 执行首页
    /// </summary>
    private void ExecuteFirstPage()
    {
        CurrentPage = 1;
    }

    /// <summary>
    /// 执行末页
    /// </summary>
    private void ExecuteLastPage()
    {
        CurrentPage = TotalPages;
    }

    /// <summary>
    /// 执行放大
    /// </summary>
    private void ExecuteZoomIn()
    {
        ZoomLevel *= 1.25;
    }

    /// <summary>
    /// 执行缩小
    /// </summary>
    private void ExecuteZoomOut()
    {
        ZoomLevel /= 1.25;
        if (ZoomLevel < 0.1)
            ZoomLevel = 0.1;
    }

    /// <summary>
    /// 执行适应宽度
    /// </summary>
    private void ExecuteFitWidth()
    {
        ZoomLevel = 800 / PageWidth;
    }

    /// <summary>
    /// 执行适应高度
    /// </summary>
    private void ExecuteFitHeight()
    {
        ZoomLevel = 600 / PageHeight;
    }

    /// <summary>
    /// 执行实际大小
    /// </summary>
    private void ExecuteActualSize()
    {
        ZoomLevel = 1.0;
    }

    /// <summary>
    /// 刷新命令状态
    /// </summary>
    private void RefreshCommandStates()
    {
        // 使用 CommandManager 强制刷新所有命令
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
