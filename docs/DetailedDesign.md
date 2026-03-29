# File Lancet 详细设计文档

## 文档信息

- **项目**: File Lancet
- **版本**: v0.2.0
- **日期**: 2026-03-29
- **状态**: 进行中

---

## 1. 总体架构设计

### 1.1 系统架构图

```
┌─────────────────────────────────────────────────────────────┐
│                      FileLancet.UI (WPF)                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   Views      │  │  ViewModels  │  │   App.xaml   │       │
│  │  (XAML)      │  │   (MVVM)     │  │   (入口)      │       │
│  └──────┬───────┘  └──────┬───────┘  └──────────────┘       │
└─────────┼─────────────────┼─────────────────────────────────┘
          │                 │
          │  数据绑定/命令调用 │
          ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                   FileLancet.Core (类库)                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   Models     │  │   Services   │  │   Interfaces │       │
│  │  (数据模型)   │  │  (业务逻辑)   │  │   (接口定义)  │       │
│  └──────────────┘  └──────┬───────┘  └──────────────┘       │
│                           │                                  │
│  ┌──────────────┐        │        ┌──────────────┐         │
│  │  Utilities   │◄───────┴───────►│   Factories  │         │
│  │  (工具类)     │                 │   (工厂类)    │         │
│  └──────────────┘                 └──────────────┘         │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 项目结构

```
FileLancet/
├── src/
│   ├── FileLancet.Core/           # 核心解析库
│   │   ├── Models/                # 数据模型
│   │   ├── Services/              # 业务服务
│   │   │   ├── EpubParser.cs
│   │   │   ├── PdfParser.cs       # v0.2.0 新增
│   │   │   ├── PdfRenderService.cs # v0.2.0 新增
│   │   │   └── ...
│   │   ├── Interfaces/            # 接口定义
│   │   │   └── IPdfRenderService.cs # v0.2.0 新增
│   │   ├── Utilities/             # 工具类
│   │   │   └── PdfRenderHelper.cs # v0.2.0 新增
│   │   └── Factories/             # 工厂类
│   └── FileLancet.UI/             # WPF 应用程序
│       ├── Views/                 # XAML 视图
│       ├── ViewModels/            # 视图模型
│       │   └── PdfPreviewViewModel.cs # v0.2.0 新增
│       ├── Converters/            # 值转换器
│       └── App.xaml               # 应用程序入口
├── tests/                         # 测试项目
└── docs/                          # 文档
```

---

## 2. v0.1.0 设计

### 2.1 核心接口设计

#### 2.1.1 解析器接口

```csharp
/// <summary>
/// 文件解析器通用接口
/// </summary>
public interface IFileLancetParser
{
    /// <summary>
    /// 判断是否可以解析指定文件
    /// </summary>
    bool CanParse(string filePath);
    
    /// <summary>
    /// 执行文件解析
    /// </summary>
    ParseResult Parse(string filePath);
    
    /// <summary>
    /// 异步解析文件
    /// </summary>
    Task<ParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default);
}
```

#### 2.1.2 解析结果

```csharp
/// <summary>
/// 解析结果
/// </summary>
public class ParseResult
{
    /// <summary>是否成功</summary>
    public bool Success { get; set; }
    
    /// <summary>错误信息</summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>文件根节点</summary>
    public FileNode RootNode { get; set; }
    
    /// <summary>文件详细信息</summary>
    public FileDetails Details { get; set; }
    
    /// <summary>原始文件路径</summary>
    public string SourcePath { get; set; }
}
```

### 2.2 数据模型设计

#### 2.2.1 文件节点

```csharp
/// <summary>
/// 文件节点类型
/// </summary>
public enum NodeType
{
    Root,           // 根节点
    Folder,         // 文件夹
    Container,      // EPUB 容器配置
    Opf,            // OPF 包文件
    Ncx,            // NCX 目录文件
    Nav,            // NAV 导航文件
    Html,           // HTML/XHTML 内容
    Css,            // 样式表
    Image,          // 图片资源
    Font,           // 字体文件
    Audio,          // 音频文件
    Video,          // 视频文件
    Script,         // JavaScript
    Other           // 其他文件
}

/// <summary>
/// 文件节点
/// </summary>
public class FileNode
{
    /// <summary>节点名称</summary>
    public string Name { get; set; }
    
    /// <summary>节点路径（相对于容器）</summary>
    public string Path { get; set; }
    
    /// <summary>节点类型</summary>
    public NodeType Type { get; set; }
    
    /// <summary>子节点列表</summary>
    public List<FileNode> Children { get; set; } = new();
    
    /// <summary>节点描述（用于状态栏提示）</summary>
    public string Description { get; set; }
    
    /// <summary>文件大小（字节）</summary>
    public long Size { get; set; }
    
    /// <summary>MIME 类型</summary>
    public string MimeType { get; set; }
    
    /// <summary>是否已加载内容</summary>
    public bool IsContentLoaded { get; set; }
    
    /// <summary>父节点引用</summary>
    public FileNode Parent { get; set; }
}
```

#### 2.2.2 文件详细信息

```csharp
/// <summary>
/// EPUB 文件详细信息
/// </summary>
public class FileDetails
{
    // 物理属性
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public string Checksum { get; set; }
    
    // EPUB 元数据
    public string Title { get; set; }
    public List<string> Authors { get; set; } = new();
    public string Publisher { get; set; }
    public string Language { get; set; }
    public string Isbn { get; set; }
    public string Description { get; set; }
    public DateTime? PublicationDate { get; set; }
    public string EpubVersion { get; set; }
    
    // 统计信息
    public int ChapterCount { get; set; }
    public int ImageCount { get; set; }
    public int StylesheetCount { get; set; }
    public int TotalFileCount { get; set; }
}
```

### 2.3 EPUB 解析器算法

#### 2.3.1 解析流程

```
┌─────────────────┐
│   输入文件路径   │
└────────┬────────┘
         ▼
┌─────────────────┐
│  验证文件存在性  │
└────────┬────────┘
         ▼
┌─────────────────┐     否     ┌──────────────┐
│  是否为ZIP格式   │───────────►│  返回错误     │
└────────┬────────┘            └──────────────┘
         │是
         ▼
┌─────────────────┐
│  打开ZIP流       │
└────────┬────────┘
         ▼
┌─────────────────┐     失败   ┌──────────────┐
│ 读取META-INF/   │───────────►│  返回错误     │
│ container.xml   │            └──────────────┘
└────────┬────────┘
         │成功
         ▼
┌─────────────────┐
│  获取OPF文件路径 │
└────────┬────────┘
         ▼
┌─────────────────┐
│  解析OPF文件     │
│  - 提取元数据    │
│  - 获取清单列表  │
│  - 获取阅读顺序  │
└────────┬────────┘
         ▼
┌─────────────────┐
│  解析目录结构    │
│  (NCX/NAV)      │
└────────┬────────┘
         ▼
┌─────────────────┐
│  构建文件树      │
└────────┬────────┘
         ▼
┌─────────────────┐
│  返回解析结果    │
└─────────────────┘
```

### 2.4 工厂模式设计

```csharp
/// <summary>
/// 解析器工厂
/// </summary>
public static class ParserFactory
{
    private static readonly List<IFileLancetParser> _parsers = new();
    
    /// <summary>
    /// 注册解析器
    /// </summary>
    public static void RegisterParser(IFileLancetParser parser)
    {
        _parsers.Add(parser);
    }
    
    /// <summary>
    /// 获取匹配的解析器
    /// </summary>
    public static IFileLancetParser GetParser(string filePath)
    {
        return _parsers.FirstOrDefault(p => p.CanParse(filePath));
    }
    
    /// <summary>
    /// 初始化默认解析器
    /// </summary>
    public static void InitializeDefaults()
    {
        RegisterParser(new EpubParser());
        RegisterParser(new PlainTextParser());
        // 后续可注册更多解析器
    }
}
```

### 2.5 视图模型设计

#### 2.5.1 主视图模型

```csharp
/// <summary>
/// 主窗口视图模型
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    // 左栏数据
    public ObservableCollection<FileNode> FileTreeNodes { get; set; }
    
    // 当前选中节点
    private FileNode _selectedNode;
    public FileNode SelectedNode
    {
        get => _selectedNode;
        set
        {
            _selectedNode = value;
            OnPropertyChanged();
            UpdateNodeDetails();
            UpdatePreview();
        }
    }
    
    // 中栏数据
    public NodeDetailsViewModel NodeDetails { get; set; }
    
    // 右栏预览数据
    public PreviewViewModel Preview { get; set; }
    
    // 状态栏信息
    public string StatusMessage { get; set; }
    public string FilePath { get; set; }
    
    // 命令
    public ICommand OpenFileCommand { get; }
    public ICommand RefreshCommand { get; }
    
    // 方法
    public async Task LoadFileAsync(string filePath);
    private void UpdateNodeDetails();
    private void UpdatePreview();
}
```

#### 2.5.2 预览视图模型

```csharp
/// <summary>
/// 预览视图模型
/// </summary>
public class PreviewViewModel
{
    // 预览类型
    public PreviewType PreviewType { get; set; }
    
    // 文本内容（HTML/CSS/XML 源码）
    public string TextContent { get; set; }
    
    // 图片内容（Base64 或 BitmapSource）
    public ImageSource ImageContent { get; set; }
    
    // HTML 内容（用于 WebView2）
    public string HtmlContent { get; set; }
    
    // 十六进制内容
    public string HexContent { get; set; }
    public bool ShowHexView { get; set; }
    
    // 二进制提示
    public bool IsBinary { get; set; }
    public string BinaryInfo { get; set; }
}

public enum PreviewType
{
    None,
    Text,
    Html,
    Image,
    Binary,
    Hex
}
```

---

## 3. v0.1.1 新增设计

### 3.1 GenericFileParser 设计

```csharp
/// <summary>
/// 通用文件解析器 - 作为兜底解析器处理任何文件格式
/// </summary>
public class GenericFileParser : IFileLancetParser
{
    /// <summary>
    /// 只要能找到文件就返回 true（作为兜底解析器）
    /// </summary>
    public bool CanParse(string filePath)
    {
        return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
    }
    
    /// <summary>
    /// 解析文件，返回简化后的文件信息
    /// </summary>
    public ParseResult Parse(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        
        var root = new FileNode
        {
            Name = fileInfo.Name,
            Path = filePath,
            Type = NodeType.Root,
            Size = fileInfo.Length,
            MimeType = GetMimeType(fileInfo.Extension)
        };
        
        var details = new FileDetails
        {
            Title = fileInfo.Name,
            FilePath = filePath,
            FileSize = fileInfo.Length,
            LastModified = fileInfo.LastWriteTime,
            CreatedTime = fileInfo.CreationTime,        // v0.1.1 新增
            FileExtension = fileInfo.Extension,         // v0.1.1 新增
            MimeType = GetMimeType(fileInfo.Extension)  // v0.1.1 新增
        };
        
        return new ParseResult
        {
            Success = true,
            RootNode = root,
            Details = details,
            SourcePath = filePath
        };
    }
}
```

### 3.2 更新后的工厂注册

```csharp
public static void InitializeDefaults()
{
    RegisterParser(new EpubParser());
    RegisterParser(new PlainTextParser());
    // ... 其他解析器
    RegisterParser(new GenericFileParser()); // 最后注册，作为兜底
}
```

### 3.3 FileDetails 扩展

```csharp
public class FileDetails
{
    // v0.1.0 已有属性
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    
    // v0.1.1 新增属性
    public DateTime CreatedTime { get; set; }
    public string FileExtension { get; set; }
    public string MimeType { get; set; }
    
    // ... 其他属性
}
```

---

## 4. v0.2.0 新增设计（PDF 支持）

### 4.1 PDF 解析器设计

#### 4.1.1 PdfParser 类

```csharp
/// <summary>
/// PDF 文件解析器
/// </summary>
public class PdfParser : IFileLancetParser
{
    /// <summary>
    /// 判断是否可以解析 PDF 文件
    /// </summary>
    public bool CanParse(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;
        
        return Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// 解析 PDF 文件
    /// </summary>
    public ParseResult Parse(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            
            // 提取文档信息
            var details = ExtractPdfDetails(document, filePath);
            
            // 构建文件树
            var rootNode = BuildPdfTree(document, filePath);
            
            return new ParseResult
            {
                Success = true,
                RootNode = rootNode,
                Details = details,
                SourcePath = filePath
            };
        }
        catch (Exception ex)
        {
            return new ParseResult
            {
                Success = false,
                ErrorMessage = $"PDF 解析失败: {ex.Message}",
                SourcePath = filePath
            };
        }
    }
    
    /// <summary>
    /// 提取 PDF 详细信息
    /// </summary>
    private PdfDetails ExtractPdfDetails(PdfDocument document, string filePath)
    {
        var info = document.Information;
        var fileInfo = new FileInfo(filePath);
        
        return new PdfDetails
        {
            Title = info.Title ?? Path.GetFileNameWithoutExtension(filePath),
            Authors = string.IsNullOrEmpty(info.Author) 
                ? new List<string>() 
                : new List<string> { info.Author },
            Subject = info.Subject,
            Keywords = info.Keywords,
            Creator = info.Creator,
            Producer = info.Producer,
            PdfVersion = document.Version,
            PageCount = document.NumberOfPages,
            IsEncrypted = document.IsEncrypted,
            FilePath = filePath,
            FileSize = fileInfo.Length,
            LastModified = fileInfo.LastWriteTime,
            CreatedTime = fileInfo.CreationTime,
            FileExtension = ".pdf",
            MimeType = "application/pdf"
        };
    }
    
    /// <summary>
    /// 构建 PDF 文件树
    /// </summary>
    private FileNode BuildPdfTree(PdfDocument document, string filePath)
    {
        var root = new FileNode
        {
            Name = Path.GetFileName(filePath),
            Path = filePath,
            Type = NodeType.PdfDocument,
            Description = $"PDF 文档 - {document.NumberOfPages} 页"
        };
        
        // 添加页面节点
        for (int i = 1; i <= document.NumberOfPages; i++)
        {
            var page = document.GetPage(i);
            root.Children.Add(new FileNode
            {
                Name = $"Page {i}",
                Path = $"{filePath}#page={i}",
                Type = NodeType.PdfPage,
                Description = $"第 {i} 页 - {page.Width:F0}x{page.Height:F0}"
            });
        }
        
        // 添加大纲/书签节点（如果有）
        var bookmarks = document.Bookmarks;
        if (bookmarks?.Count > 0)
        {
            var outlineNode = new FileNode
            {
                Name = "大纲",
                Path = $"{filePath}#outline",
                Type = NodeType.PdfOutline
            };
            BuildOutlineTree(bookmarks, outlineNode);
            root.Children.Add(outlineNode);
        }
        
        return root;
    }
    
    /// <summary>
    /// 递归构建大纲树
    /// </summary>
    private void BuildOutlineTree(IReadOnlyList<BookmarkNode> bookmarks, FileNode parent)
    {
        foreach (var bookmark in bookmarks)
        {
            var node = new FileNode
            {
                Name = bookmark.Title,
                Path = $"{parent.Path}/{bookmark.Title}",
                Type = NodeType.PdfOutline,
                Description = $"跳转到第 {bookmark.PageNumber} 页"
            };
            
            if (bookmark.Children?.Count > 0)
            {
                BuildOutlineTree(bookmark.Children, node);
            }
            
            parent.Children.Add(node);
        }
    }
}
```

#### 4.1.2 NodeType 扩展

```csharp
/// <summary>
/// 文件节点类型（v0.2.0 扩展）
/// </summary>
public enum NodeType
{
    // ... 原有类型
    
    // PDF 相关类型
    PdfDocument,    // PDF 文档根节点
    PdfPage,        // PDF 页面
    PdfOutline,     // PDF 书签/大纲
    PdfFont,        // PDF 字体
    PdfImage,       // PDF 内嵌图片
}
```

### 4.2 PDF 详情模型

```csharp
/// <summary>
/// PDF 文件详细信息
/// </summary>
public class PdfDetails : FileDetails
{
    // PDF 特有属性
    public string PdfVersion { get; set; }
    public int PageCount { get; set; }
    public bool IsEncrypted { get; set; }
    public string Creator { get; set; }
    public string Producer { get; set; }
    public string Subject { get; set; }
    public string Keywords { get; set; }
    
    // 页面信息列表
    public List<PdfPageInfo> Pages { get; set; } = new();
}

/// <summary>
/// PDF 页面信息
/// </summary>
public class PdfPageInfo
{
    public int PageNumber { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int Rotation { get; set; }
    public string MediaBox { get; set; }
}
```

### 4.3 PDF 渲染服务设计

#### 4.3.1 IPdfRenderService 接口

```csharp
/// <summary>
/// PDF 渲染服务接口
/// </summary>
public interface IPdfRenderService
{
    /// <summary>
    /// 渲染指定页面为位图
    /// </summary>
    Task<BitmapSource> RenderPageAsync(string pdfPath, int pageNumber, 
        double scale = 1.0, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 渲染页面缩略图
    /// </summary>
    Task<BitmapSource> RenderThumbnailAsync(string pdfPath, int pageNumber, 
        int maxWidth = 200, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 提取页面文本内容
    /// </summary>
    Task<string> ExtractPageTextAsync(string pdfPath, int pageNumber, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取页面尺寸
    /// </summary>
    Task<(double Width, double Height)> GetPageSizeAsync(string pdfPath, int pageNumber);
}
```

#### 4.3.2 PdfRenderService 实现

```csharp
/// <summary>
/// PDF 渲染服务实现
/// </summary>
public class PdfRenderService : IPdfRenderService
{
    /// <summary>
    /// 渲染指定页面为位图
    /// </summary>
    public async Task<BitmapSource> RenderPageAsync(string pdfPath, int pageNumber, 
        double scale = 1.0, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var document = PdfDocument.Open(pdfPath);
            var page = document.GetPage(pageNumber);
            
            // 计算渲染尺寸
            int width = (int)(page.Width * scale * 2);  // 2x for better quality
            int height = (int)(page.Height * scale * 2);
            
            // 使用 SkiaSharp 渲染
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            
            // 白色背景
            canvas.Clear(SKColors.White);
            
            // 渲染 PDF 页面内容
            var pageRenderer = new PdfPageRenderer(page);
            pageRenderer.Render(canvas, width, height);
            
            // 转换为 WPF BitmapSource
            return ConvertToBitmapSource(bitmap);
        }, cancellationToken);
    }
    
    /// <summary>
    /// 渲染页面缩略图
    /// </summary>
    public async Task<BitmapSource> RenderThumbnailAsync(string pdfPath, int pageNumber, 
        int maxWidth = 200, CancellationToken cancellationToken = default)
    {
        var (pageWidth, pageHeight) = await GetPageSizeAsync(pdfPath, pageNumber);
        
        // 计算缩放比例
        double scale = maxWidth / pageWidth;
        
        return await RenderPageAsync(pdfPath, pageNumber, scale, cancellationToken);
    }
    
    /// <summary>
    /// 提取页面文本内容
    /// </summary>
    public async Task<string> ExtractPageTextAsync(string pdfPath, int pageNumber, 
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var document = PdfDocument.Open(pdfPath);
            var page = document.GetPage(pageNumber);
            return page.Text;
        }, cancellationToken);
    }
    
    /// <summary>
    /// 获取页面尺寸
    /// </summary>
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
    /// 将 SKBitmap 转换为 WPF BitmapSource
    /// </summary>
    private BitmapSource ConvertToBitmapSource(SKBitmap bitmap)
    {
        // 实现转换逻辑
        // ...
    }
}
```

### 4.4 PDF 预览视图模型

```csharp
/// <summary>
/// PDF 预览视图模型
/// </summary>
public class PdfPreviewViewModel : INotifyPropertyChanged
{
    private readonly IPdfRenderService _renderService;
    
    // 当前 PDF 路径
    private string _pdfPath;
    public string PdfPath
    {
        get => _pdfPath;
        set
        {
            _pdfPath = value;
            OnPropertyChanged();
            LoadDocumentAsync();
        }
    }
    
    // 当前页码
    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (value >= 1 && value <= TotalPages)
            {
                _currentPage = value;
                OnPropertyChanged();
                RenderCurrentPageAsync();
            }
        }
    }
    
    // 总页数
    private int _totalPages;
    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            _totalPages = value;
            OnPropertyChanged();
        }
    }
    
    // 当前页面图像
    private BitmapSource _pageImage;
    public BitmapSource PageImage
    {
        get => _pageImage;
        private set
        {
            _pageImage = value;
            OnPropertyChanged();
        }
    }
    
    // 缩放级别
    private double _zoomLevel = 1.0;
    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            _zoomLevel = value;
            OnPropertyChanged();
            RenderCurrentPageAsync();
        }
    }
    
    // 是否显示文本模式
    private bool _showTextMode;
    public bool ShowTextMode
    {
        get => _showTextMode;
        set
        {
            _showTextMode = value;
            OnPropertyChanged();
            if (value)
                LoadTextContentAsync();
            else
                RenderCurrentPageAsync();
        }
    }
    
    // 文本内容
    private string _textContent;
    public string TextContent
    {
        get => _textContent;
        private set
        {
            _textContent = value;
            OnPropertyChanged();
        }
    }
    
    // 命令
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand FirstPageCommand { get; }
    public ICommand LastPageCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand FitWidthCommand { get; }
    public ICommand FitHeightCommand { get; }
    
    // 方法
    private async Task LoadDocumentAsync();
    private async Task RenderCurrentPageAsync();
    private async Task LoadTextContentAsync();
}
```

### 4.5 工厂注册更新

```csharp
public static void InitializeDefaults()
{
    RegisterParser(new EpubParser());
    RegisterParser(new PlainTextParser());
    RegisterParser(new PdfParser());        // v0.2.0 新增
    RegisterParser(new GenericFileParser()); // 最后注册，作为兜底
}
```

---

## 5. 界面布局设计

### 5.1 双栏布局

```xml
<Grid Grid.Row="1" Margin="5">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="350" MinWidth="250" MaxWidth="500"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*" MinWidth="500"/>
    </Grid.ColumnDefinitions>

    <!-- 左栏：文件树 + 详情面板（上下布局） -->
    <Grid Grid.Column="0">
        <Grid.RowDefinitions>
            <RowDefinition Height="*" MinHeight="200"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="250" MinHeight="150" MaxHeight="400"/>
        </Grid.RowDefinitions>

        <!-- 上部：文件树 -->
        <Border Grid.Row="0">
            <TreeView ItemsSource="{Binding FileTreeNodes}"
                      SelectedItemChanged="TreeView_SelectedItemChanged"/>
        </Border>

        <!-- 分隔线 -->
        <GridSplitter Grid.Row="1" Height="4"/>

        <!-- 下部：详情面板 -->
        <Border Grid.Row="2">
            <!-- 详情内容 -->
        </Border>
    </Grid>

    <!-- 分隔线 -->
    <GridSplitter Grid.Column="1" Width="4"/>

    <!-- 右栏：预览（加宽） -->
    <Border Grid.Column="2">
        <!-- 预览内容 -->
    </Border>
</Grid>
```

### 5.2 PDF 预览控件

```xml
<!-- PDF 预览视图 -->
<UserControl x:Class="FileLancet.UI.Views.PdfPreviewView">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- 工具栏 -->
        <ToolBar Grid.Row="0">
            <Button Command="{Binding PreviousPageCommand}" Content="◀"/>
            <TextBlock Text="{Binding CurrentPage, StringFormat='{}{0}/'}"/>
            <TextBlock Text="{Binding TotalPages}"/>
            <Button Command="{Binding NextPageCommand}" Content="▶"/>
            <Separator/>
            <Button Command="{Binding ZoomOutCommand}" Content="-"/>
            <TextBlock Text="{Binding ZoomLevel, StringFormat='{}{0:P0}'}"/>
            <Button Command="{Binding ZoomInCommand}" Content="+"/>
            <Separator/>
            <ToggleButton IsChecked="{Binding ShowTextMode}" Content="文本模式"/>
        </ToolBar>
        
        <!-- 预览区域 -->
        <ScrollViewer Grid.Row="1" HorizontalScrollBarVisibility="Auto">
            <ContentControl>
                <ContentControl.Style>
                    <Style TargetType="ContentControl">
                        <Style.Triggers>
                            <DataTrigger Binding="{Binding ShowTextMode}" Value="False">
                                <Setter Property="Content">
                                    <Setter.Value>
                                        <Image Source="{Binding PageImage}" 
                                               Stretch="None"/>
                                    </Setter.Value>
                                </Setter>
                            </DataTrigger>
                            <DataTrigger Binding="{Binding ShowTextMode}" Value="True">
                                <Setter Property="Content">
                                    <Setter.Value>
                                        <TextBox Text="{Binding TextContent}" 
                                                 IsReadOnly="True"
                                                 ScrollViewer.VerticalScrollBarVisibility="Auto"/>
                                    </Setter.Value>
                                </Setter>
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </ContentControl.Style>
            </ContentControl>
        </ScrollViewer>
        
        <!-- 状态栏 -->
        <StatusBar Grid.Row="2">
            <TextBlock Text="{Binding StatusMessage}"/>
        </StatusBar>
    </Grid>
</UserControl>
```

---

## 6. 测试设计

### 6.1 单元测试覆盖率目标

| 模块 | 覆盖率目标 | 实际覆盖率 |
|------|-----------|-----------|
| Models | ≥ 95% | ~98% |
| Services | ≥ 90% | ~92% |
| Factories | ≥ 90% | ~95% |
| ViewModels | ≥ 85% | ~88% |
| Utilities | ≥ 85% | ~93% |
| **整体** | **≥ 90%** | **~92%** |

### 6.2 测试用例分布

| 版本 | 测试数量 | 主要内容 |
|------|---------|---------|
| v0.1.0 | 129 | 核心解析、界面、预览、扩展性、XML工具、十六进制 |
| v0.1.1 | 8 | 通用文件解析器 |
| v0.2.0 | 42 | PDF解析引擎、PDF渲染预览、PDF文件树与详情 |
| **总计** | **179** | - |

### 6.3 PDF 测试用例设计

#### 6.3.1 PdfParser 解析器测试（TC-801 ~ TC-820）

| 测试编号 | 测试场景 | 测试内容 | 优先级 |
|---------|---------|---------|--------|
| TC-801 | PdfParser CanParse 有效PDF | 验证 .pdf 文件识别 | 高 |
| TC-802 | PdfParser CanParse 非PDF | 验证非 PDF 文件返回 false | 高 |
| TC-803 | PdfParser CanParse 空路径 | 验证空路径处理 | 高 |
| TC-804 | PdfParser CanParse null路径 | 验证 null 路径处理 | 高 |
| TC-805 | PdfParser CanParse 不存在文件 | 验证不存在文件处理 | 高 |
| TC-806 | PdfParser Parse 标准PDF | 验证标准 PDF 解析成功 | 高 |
| TC-807 | PdfParser Parse 提取元数据 | 验证标题、作者、页数等提取 | 高 |
| TC-808 | PdfParser Parse 多作者PDF | 验证多作者提取 | 中 |
| TC-809 | PdfParser Parse 中文PDF | 验证中文元数据提取 | 中 |
| TC-810 | PdfParser Parse 加密PDF | 验证加密 PDF 检测 | 高 |
| TC-811 | PdfParser Parse 损坏PDF | 验证损坏文件错误处理 | 高 |
| TC-812 | PdfParser Parse 不存在文件 | 验证不存在文件错误处理 | 高 |
| TC-813 | PdfParser Parse 空PDF | 验证空 PDF 文件处理 | 中 |
| TC-814 | PdfParser 构建文件树 | 验证页面节点生成 | 高 |
| TC-815 | PdfParser 单页PDF | 验证单页 PDF 文件树 | 中 |
| TC-816 | PdfParser 多页PDF | 验证多页 PDF 文件树（100+页） | 中 |
| TC-817 | PdfParser 大纲提取 | 验证书签/大纲提取 | 中 |
| TC-818 | PdfParser 嵌套大纲 | 验证嵌套书签提取 | 中 |
| TC-819 | PdfParser 无大纲PDF | 验证无书签 PDF 处理 | 中 |
| TC-820 | PdfParser ParseAsync 异步 | 验证异步解析 | 高 |

#### 6.3.2 PdfRenderService 渲染服务测试（TC-821 ~ TC-835）

| 测试编号 | 测试场景 | 测试内容 | 优先级 |
|---------|---------|---------|--------|
| TC-821 | RenderPage 第一页 | 验证首页渲染 | 高 |
| TC-822 | RenderPage 中间页 | 验证中间页渲染 | 高 |
| TC-823 | RenderPage 最后一页 | 验证末页渲染 | 高 |
| TC-824 | RenderPage 无效页码 | 验证无效页码处理 | 高 |
| TC-825 | RenderPage 缩放1x | 验证实际大小渲染 | 中 |
| TC-826 | RenderPage 缩放2x | 验证2倍放大渲染 | 中 |
| TC-827 | RenderPage 缩放0.5x | 验证缩小渲染 | 中 |
| TC-828 | RenderThumbnail 默认尺寸 | 验证默认缩略图 | 中 |
| TC-829 | RenderThumbnail 自定义尺寸 | 验证自定义尺寸缩略图 | 中 |
| TC-830 | ExtractText 纯文本PDF | 验证纯文本提取 | 高 |
| TC-831 | ExtractText 图文混排PDF | 验证图文混排文本提取 | 中 |
| TC-832 | ExtractText 扫描PDF | 验证扫描件文本提取（OCR） | 低 |
| TC-833 | GetPageSize | 验证页面尺寸获取 | 中 |
| TC-834 | RenderPage 取消令牌 | 验证取消操作 | 中 |
| TC-835 | RenderPage 并发渲染 | 验证多页面并发渲染 | 低 |

#### 6.3.3 PdfPreviewViewModel 视图模型测试（TC-836 ~ TC-850）

| 测试编号 | 测试场景 | 测试内容 | 优先级 |
|---------|---------|---------|--------|
| TC-836 | 初始化VM | 验证初始状态 | 高 |
| TC-837 | 加载PDF | 验证 PDF 加载 | 高 |
| TC-838 | 当前页变更 | 验证页面切换 | 高 |
| TC-839 | PreviousPageCommand | 验证上一页命令 | 高 |
| TC-840 | NextPageCommand | 验证下一页命令 | 高 |
| TC-841 | FirstPageCommand | 验证首页命令 | 高 |
| TC-842 | LastPageCommand | 验证末页命令 | 高 |
| TC-843 | ZoomInCommand | 验证放大命令 | 中 |
| TC-844 | ZoomOutCommand | 验证缩小命令 | 中 |
| TC-845 | FitWidthCommand | 验证适应宽度 | 中 |
| TC-846 | FitHeightCommand | 验证适应高度 | 中 |
| TC-847 | ShowTextMode 切换 | 验证文本模式切换 | 中 |
| TC-848 | 边界页码测试 | 验证页码边界（<1, >TotalPages） | 高 |
| TC-849 | 属性变更通知 | 验证 INotifyPropertyChanged | 高 |
| TC-850 | 异常处理 | 验证渲染异常处理 | 高 |

#### 6.3.4 PdfDetails 数据模型测试（TC-851 ~ TC-860）

| 测试编号 | 测试场景 | 测试内容 | 优先级 |
|---------|---------|---------|--------|
| TC-851 | PdfDetails 属性设置 | 验证所有属性可设置 | 高 |
| TC-852 | PdfDetails 继承FileDetails | 验证继承关系 | 高 |
| TC-853 | PdfPageInfo 属性验证 | 验证页面信息属性 | 中 |
| TC-854 | PdfDetails Pages列表 | 验证页面列表操作 | 中 |
| TC-855 | PdfDetails 空值处理 | 验证空值安全 | 中 |

#### 6.3.5 集成测试（TC-856 ~ TC-870）

| 测试编号 | 测试场景 | 测试内容 | 优先级 |
|---------|---------|---------|--------|
| TC-856 | 完整PDF解析流程 | 端到端解析测试 | 高 |
| TC-857 | PDF 1.4版本 | 验证 PDF 1.4 兼容性 | 中 |
| TC-858 | PDF 1.7版本 | 验证 PDF 1.7 兼容性 | 中 |
| TC-859 | PDF/A标准 | 验证 PDF/A 兼容性 | 低 |
| TC-860 | 大文件PDF | 验证 100MB+ PDF 处理 | 中 |
| TC-861 | 多线程安全 | 验证多线程解析安全 | 中 |
| TC-862 | 内存泄漏 | 验证无内存泄漏 | 高 |
| TC-863 | 渲染性能 | 验证渲染时间 < 500ms | 高 |
| TC-864 | 快速翻页 | 验证快速翻页响应 | 中 |
| TC-865 | 工厂注册 | 验证 PdfParser 工厂注册 | 高 |

---

## 7. 接口汇总

### 7.1 核心接口

| 接口名 | 职责 | 关键方法 |
|-------|------|---------|
| IFileLancetParser | 文件解析器 | CanParse, Parse, ParseAsync |
| IContentLoader | 内容加载 | LoadContentAsync, LoadTextAsync |
| IPreviewService | 预览服务 | GetPreviewAsync |
| **IPdfRenderService** | **PDF 渲染服务** | **RenderPageAsync, RenderThumbnailAsync, ExtractPageTextAsync** |

### 7.2 数据模型

| 类名 | 职责 | 关键属性 |
|-----|------|---------|
| FileNode | 文件树节点 | Name, Path, Type, Children |
| FileDetails | 文件详情 | Title, Authors, FileSize, Metadata, FileExtension, MimeType, CreatedTime |
| ParseResult | 解析结果 | Success, RootNode, Details |
| **PdfDetails** | **PDF 详情** | **PdfVersion, PageCount, PageSize, Creator, Producer, IsEncrypted** |
| **PdfPageInfo** | **PDF 页面信息** | **PageNumber, Width, Height, Rotation** |

### 7.3 视图模型

| 类名 | 职责 | 关键属性 |
|-----|------|---------|
| MainViewModel | 主窗口 VM | FileTreeNodes, SelectedNode, Commands |
| NodeDetailsViewModel | 详情面板 VM | Properties, Metadata |
| PreviewViewModel | 预览面板 VM | PreviewType, Content, HexContent, ShowHexView |
| **PdfPreviewViewModel** | **PDF 预览 VM** | **CurrentPage, TotalPages, PageImage, ZoomLevel, ShowTextMode** |

### 7.4 工具类

| 类名 | 职责 | 关键方法 |
|-----|------|---------|
| XmlParserHelper | XML 解析辅助 | ParseWithNamespaceFix, GetZipEntry, GetElementValueByLocalName |
| PerformanceOptimizer | 性能优化 | CreateCache, MonitorMemory, ThrottleAsync |
| **PdfRenderHelper** | **PDF 渲染辅助** | **RenderPageToBitmap, ExtractPageText** |

### 7.5 解析器类

| 类名 | 职责 | 支持格式 |
|-----|------|---------|
| EpubParser | EPUB 解析 | .epub |
| PlainTextParser | 文本解析 | .txt |
| **PdfParser** | **PDF 解析** | **.pdf** |
| GenericFileParser | 通用解析 | 任何文件 |
