# File Lancet 详细设计文档

## 文档信息

- **版本**: v1.0
- **日期**: 2026-03-26
- **状态**: 设计阶段

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
│   │   ├── Interfaces/            # 接口定义
│   │   ├── Utilities/             # 工具类
│   │   └── Factories/             # 工厂类
│   └── FileLancet.UI/             # WPF 应用程序
│       ├── Views/                 # XAML 视图
│       ├── ViewModels/            # 视图模型
│       ├── Converters/            # 值转换器
│       └── App.xaml               # 应用程序入口
├── tests/
│   ├── FileLancet.Core.Tests/     # 核心库单元测试
│   └── FileLancet.UI.Tests/       # UI 测试
└── docs/                          # 文档
```

---

## 2. 阶段一：核心地基与 EPUB 解析引擎

### 2.1 目标

构建不依赖 UI 的控制台级解析内核，实现 EPUB 文件的完整解析能力。

### 2.2 核心接口设计

#### 2.2.1 解析器接口

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

#### 2.2.2 解析结果

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

### 2.3 数据模型设计

#### 2.3.1 文件节点

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

#### 2.3.2 文件详细信息

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

### 2.4 EPUB 解析器算法

#### 2.4.1 解析流程

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

#### 2.4.2 懒加载策略

```csharp
/// <summary>
/// 内容加载器
/// </summary>
public interface IContentLoader
{
    /// <summary>
    /// 异步加载节点内容
    /// </summary>
    Task<byte[]> LoadContentAsync(FileNode node);
    
    /// <summary>
    /// 异步加载文本内容
    /// </summary>
    Task<string> LoadTextAsync(FileNode node);
}

/// <summary>
/// EPUB 内容加载器实现
/// </summary>
public class EpubContentLoader : IContentLoader
{
    private readonly string _epubPath;
    private readonly Dictionary<string, ZipArchiveEntry> _entryCache;
    
    // 实现懒加载逻辑：
    // 1. 首次访问时打开 ZIP 流
    // 2. 根据节点路径定位 Entry
    // 3. 读取内容并缓存
    // 4. 使用完成后及时释放流
}
```

### 2.5 工厂模式设计

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
        // 后续可注册更多解析器
    }
}
```

### 2.6 阶段一测试用例

| 测试编号 | 测试场景 | 输入 | 预期输出 | 验证点 |
|---------|---------|------|---------|--------|
| TC-001 | 标准 EPUB 2.0 解析 | 有效 EPUB 2.0 文件 | ParseResult.Success = true | 元数据、文件树完整 |
| TC-002 | 标准 EPUB 3.0 解析 | 有效 EPUB 3.0 文件 | ParseResult.Success = true | 支持 NAV 导航 |
| TC-003 | 损坏 ZIP 处理 | 损坏的 EPUB 文件 | ParseResult.Success = false | 错误信息明确 |
| TC-004 | 非 EPUB 文件 | .txt 文件 | 返回 null 或错误 | CanParse 返回 false |
| TC-005 | 大文件解析 | >100MB EPUB | 解析成功，内存占用合理 | 懒加载生效 |
| TC-006 | 异步解析取消 | 大文件 + CancellationToken | 操作取消，资源释放 | 无内存泄漏 |

### 2.7 阶段一交付物

1. **代码交付**
   - FileLancet.Core 类库项目
   - 完整的 EPUB 解析器实现
   - 数据模型定义

2. **测试交付**
   - 单元测试项目（覆盖率 ≥ 80%）
   - 测试用例执行报告

3. **文档交付**
   - 接口 API 文档
   - 解析算法说明

4. **可运行程序**
   - 控制台测试程序，支持命令行解析 EPUB 并输出 JSON

---

## 3. 阶段二：三栏式界面实现

### 3.1 目标

使用 WPF 构建三栏式界面，实现数据模型与视图的绑定。

### 3.2 视图模型设计

#### 3.2.1 主视图模型

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
    public ICommand ExportCommand { get; }
    
    // 方法
    public async Task LoadFileAsync(string filePath);
    private void UpdateNodeDetails();
    private void UpdatePreview();
}
```

#### 3.2.2 节点详情视图模型

```csharp
/// <summary>
/// 节点详情视图模型
/// </summary>
public class NodeDetailsViewModel
{
    // 基本信息
    public string NodeName { get; set; }
    public string NodeType { get; set; }
    public string NodePath { get; set; }
    
    // 物理属性
    public string FileSize { get; set; }
    public string MimeType { get; set; }
    public string LastModified { get; set; }
    
    // EPUB 元数据（仅对根节点显示）
    public bool IsEpubMetadataVisible { get; set; }
    public string Title { get; set; }
    public string Authors { get; set; }
    public string Publisher { get; set; }
    public string Language { get; set; }
    public string Isbn { get; set; }
    public string EpubVersion { get; set; }
    
    // 属性列表（用于 DataGrid 绑定）
    public ObservableCollection<PropertyItem> Properties { get; set; }
}

public class PropertyItem
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Category { get; set; } // 物理/逻辑
}
```

#### 3.2.3 预览视图模型

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
    Binary
}
```

### 3.3 视图布局设计

#### 3.3.1 主窗口 XAML 结构

```xml
<Window x:Class="FileLancet.UI.Views.MainWindow"
        Title="File Lancet" 
        Width="1400" Height="900">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>      <!-- 菜单栏 -->
            <RowDefinition Height="*"/>         <!-- 主内容区 -->
            <RowDefinition Height="Auto"/>      <!-- 状态栏 -->
        </Grid.RowDefinitions>
        
        <!-- 菜单栏 -->
        <Menu Grid.Row="0">
            <MenuItem Header="文件">
                <MenuItem Header="打开" Command="{Binding OpenFileCommand}"/>
                <MenuItem Header="导出" Command="{Binding ExportCommand}"/>
                <Separator/>
                <MenuItem Header="退出"/>
            </MenuItem>
        </Menu>
        
        <!-- 三栏主内容 -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="250" MinWidth="150" MaxWidth="400"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*" MinWidth="300"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="400" MinWidth="250" MaxWidth="600"/>
            </Grid.ColumnDefinitions>
            
            <!-- 左栏：文件树 -->
            <TreeView Grid.Column="0" 
                      ItemsSource="{Binding FileTreeNodes}"
                      SelectedItem="{Binding SelectedNode}"/>
            
            <!-- 分隔线 -->
            <GridSplitter Grid.Column="1" Width="5" HorizontalAlignment="Stretch"/>
            
            <!-- 中栏：详情面板 -->
            <ContentControl Grid.Column="2" Content="{Binding NodeDetails}"/>
            
            <!-- 分隔线 -->
            <GridSplitter Grid.Column="3" Width="5" HorizontalAlignment="Stretch"/>
            
            <!-- 右栏：预览 -->
            <ContentControl Grid.Column="4" Content="{Binding Preview}"/>
        </Grid>
        
        <!-- 状态栏 -->
        <StatusBar Grid.Row="2">
            <TextBlock Text="{Binding StatusMessage}"/>
            <Separator/>
            <TextBlock Text="{Binding FilePath}"/>
        </StatusBar>
    </Grid>
</Window>
```

### 3.4 阶段二测试用例

| 测试编号 | 测试场景 | 操作 | 预期结果 | 验证点 |
|---------|---------|------|---------|--------|
| TC-007 | 文件加载 | 点击打开菜单选择 EPUB | 左侧显示文件树 | 树结构正确 |
| TC-008 | 节点选择 | 点击树节点 | 中栏显示详情 | 属性匹配 |
| TC-009 | 栏宽调整 | 拖拽分隔线 | 栏宽实时变化 | 布局自适应 |
| TC-010 | 空状态 | 未加载文件 | 显示提示信息 | UI 友好 |
| TC-011 | 大数据量 | 加载大文件 | 界面不卡顿 | 虚拟化生效 |

### 3.5 阶段二交付物

1. **代码交付**
   - FileLancet.UI WPF 项目
   - 完整的 ViewModels 实现
   - XAML 视图文件

2. **测试交付**
   - UI 单元测试
   - 手动测试清单

3. **可运行程序**
   - 可加载 EPUB 并显示三栏界面的桌面应用

---

## 4. 阶段三：内容预览与交互完善

### 4.1 目标

实现右栏预览功能，完善拖拽、文件关联等交互特性。

### 4.2 预览功能设计

#### 4.2.1 预览服务接口

```csharp
/// <summary>
/// 预览服务接口
/// </summary>
public interface IPreviewService
{
    /// <summary>
    /// 根据节点类型获取预览内容
    /// </summary>
    Task<PreviewViewModel> GetPreviewAsync(FileNode node);
}

/// <summary>
/// 预览服务实现
/// </summary>
public class PreviewService : IPreviewService
{
    private readonly IContentLoader _contentLoader;
    
    public async Task<PreviewViewModel> GetPreviewAsync(FileNode node)
    {
        return node.Type switch
        {
            NodeType.Html => await PreviewHtmlAsync(node),
            NodeType.Css => await PreviewTextAsync(node, "css"),
            NodeType.Image => await PreviewImageAsync(node),
            _ => PreviewBinary(node)
        };
    }
}
```

#### 4.2.2 WebView2 集成

```csharp
/// <summary>
/// HTML 预览控件（封装 WebView2）
/// </summary>
public class HtmlPreviewControl : UserControl
{
    private WebView2 _webView;
    
    /// <summary>
    /// 加载 HTML 内容（支持从 ZIP 流加载资源）
    /// </summary>
    public async Task LoadHtmlAsync(string htmlContent, string basePath);
    
    /// <summary>
    /// 处理资源请求（拦截图片、CSS 等资源加载）
    /// </summary>
    private void OnWebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e);
}
```

### 4.3 拖拽功能设计

```csharp
/// <summary>
/// 拖拽处理服务
/// </summary>
public class DragDropService
{
    /// <summary>
    /// 启用窗口拖拽接收
    /// </summary>
    public void EnableDragDrop(Window window, Func<string, Task> onFileDropped)
    {
        window.AllowDrop = true;
        window.PreviewDragOver += (s, e) =>
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        };
        
        window.Drop += async (s, e) =>
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                    await onFileDropped(files[0]);
            }
        };
    }
}
```

### 4.4 阶段三测试用例

| 测试编号 | 测试场景 | 操作 | 预期结果 | 验证点 |
|---------|---------|------|---------|--------|
| TC-012 | HTML 预览 | 选择 HTML 节点 | 右栏渲染网页 | 样式图片正确 |
| TC-013 | 图片预览 | 选择图片节点 | 显示图片 | 缩放适应 |
| TC-014 | 代码高亮 | 选择 CSS 节点 | 语法高亮显示 | 关键字着色 |
| TC-015 | 拖拽打开 | 拖拽 EPUB 到窗口 | 文件加载成功 | 反馈明确 |
| TC-016 | 文件关联 | 双击 EPUB 文件 | 应用启动并加载 | 参数传递正确 |
| TC-017 | 错误处理 | 打开损坏文件 | 显示错误提示 | 不崩溃 |
| TC-018 | 加载状态 | 打开大文件 | 显示进度指示 | 防止假死 |

### 4.5 阶段三交付物

1. **代码交付**
   - 预览服务实现
   - WebView2 封装控件
   - 拖拽和文件关联功能

2. **测试交付**
   - 集成测试用例
   - 性能测试报告

3. **可运行程序**
   - File Lancet v0.1 Beta
   - 完整的预览和交互功能

---

## 5. 阶段四：扩展性验证与优化

### 5.1 目标

验证架构灵活性，实现多格式支持基础，优化性能。

### 5.2 扩展架构设计

#### 5.2.1 解析器基类

```csharp
/// <summary>
/// 解析器抽象基类
/// </summary>
public abstract class BaseParser : IFileLancetParser
{
    protected ILogger Logger { get; }
    protected IContentLoader ContentLoader { get; }
    
    public abstract bool CanParse(string filePath);
    public abstract ParseResult Parse(string filePath);
    
    /// <summary>
    /// 通用异常处理
    /// </summary>
    protected ParseResult HandleException(Exception ex, string filePath)
    {
        Logger.LogError(ex, "解析文件失败: {FilePath}", filePath);
        return new ParseResult
        {
            Success = false,
            ErrorMessage = $"解析失败: {ex.Message}",
            SourcePath = filePath
        };
    }
    
    /// <summary>
    /// 构建通用文件节点
    /// </summary>
    protected FileNode CreateNode(string name, string path, NodeType type)
    {
        return new FileNode
        {
            Name = name,
            Path = path,
            Type = type,
            Description = GetNodeDescription(type)
        };
    }
    
    protected abstract string GetNodeDescription(NodeType type);
}
```

#### 5.2.2 文本解析器示例

```csharp
/// <summary>
/// 纯文本解析器（用于验证扩展性）
/// </summary>
public class PlainTextParser : BaseParser
{
    public override bool CanParse(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".txt", StringComparison.OrdinalIgnoreCase);
    }
    
    public override ParseResult Parse(string filePath)
    {
        try
        {
            var root = CreateNode(Path.GetFileName(filePath), "", NodeType.Root);
            var content = CreateNode("content", "content", NodeType.Other);
            root.Children.Add(content);
            
            return new ParseResult
            {
                Success = true,
                RootNode = root,
                Details = new FileDetails
                {
                    Title = Path.GetFileNameWithoutExtension(filePath),
                    FilePath = filePath,
                    FileSize = new FileInfo(filePath).Length
                }
            };
        }
        catch (Exception ex)
        {
            return HandleException(ex, filePath);
        }
    }
    
    protected override string GetNodeDescription(NodeType type) => "文本文件节点";
}
```

### 5.3 性能优化策略

#### 5.3.1 内存优化

```csharp
/// <summary>
/// 内存管理策略
/// </summary>
public class MemoryOptimization
{
    /// <summary>
    /// ZIP 流使用模式
    /// </summary>
    public void UseZipStream(string epubPath, Action<ZipArchive> action)
    {
        using var stream = File.OpenRead(epubPath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        action(archive);
        // 流自动释放
    }
    
    /// <summary>
    /// 大文件分块读取
    /// </summary>
    public async Task ReadLargeFileAsync(string path, int bufferSize = 81920)
    {
        await using var stream = File.OpenRead(path);
        var buffer = new byte[bufferSize];
        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            // 处理数据块
            ProcessChunk(buffer, read);
        }
    }
    
    /// <summary>
    /// 图片缓存策略（LRU）
    /// </summary>
    public class ImageCache : MemoryCache<string, BitmapSource>
    {
        private readonly int _maxCacheSize;
        
        public ImageCache(int maxCacheSize = 50) : base(maxCacheSize) { }
        
        protected override void OnItemEvicted(string key, BitmapSource value)
        {
            value.Freeze(); // 释放资源
        }
    }
}
```

#### 5.3.2 异步优化

```csharp
/// <summary>
/// 异步操作优化
/// </summary>
public class AsyncOptimization
{
    /// <summary>
    /// 并行解析多个文件
    /// </summary>
    public async Task<ParseResult[]> ParseMultipleAsync(string[] filePaths, int maxParallel = 4)
    {
        var semaphore = new SemaphoreSlim(maxParallel);
        var tasks = filePaths.Select(async path =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await ParseAsync(path);
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        return await Task.WhenAll(tasks);
    }
    
    /// <summary>
    /// 后台加载进度报告
    /// </summary>
    public async Task LoadWithProgressAsync(string filePath, IProgress<double> progress)
    {
        await Task.Run(async () =>
        {
            // 分段加载，报告进度
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(10);
                progress.Report(i / 100.0);
            }
        });
    }
}
```

### 5.4 发布流程设计

```
┌─────────────────┐
│   编译 Release   │
└────────┬────────┘
         ▼
┌─────────────────┐
│  运行单元测试    │
└────────┬────────┘
         ▼
┌─────────────────┐     失败   ┌──────────────┐
│   测试通过？     │───────────►│   修复问题    │
└────────┬────────┘            └──────────────┘
         │是                    ▲
         ▼                      │
┌─────────────────┐             │
│  生成安装包     │             │
│  - MSI (x64)   │─────────────┘
│  - ZIP 便携版   │
└────────┬────────┘
         ▼
┌─────────────────┐
│   签名验证      │
└────────┬────────┘
         ▼
┌─────────────────┐
│   发布交付      │
└─────────────────┘
```

### 5.5 阶段四测试用例

| 测试编号 | 测试场景 | 操作 | 预期结果 | 验证点 |
|---------|---------|------|---------|--------|
| TC-019 | 扩展性验证 | 加载 .txt 文件 | 正确显示结构 | 新解析器生效 |
| TC-020 | 内存泄漏 | 连续打开 100 个大文件 | 内存稳定 | 无泄漏 |
| TC-021 | 并发性能 | 同时解析多个文件 | 完成时间合理 | 线程安全 |
| TC-022 | 安装包测试 | 安装 MSI | 正常安装运行 | 注册表正确 |
| TC-023 | 便携版测试 | 解压 ZIP 运行 | 无需安装 | 依赖完整 |

### 5.6 阶段四交付物

1. **代码交付**
   - BaseParser 抽象基类
   - PlainTextParser 示例实现
   - 性能优化代码

2. **测试交付**
   - 扩展性验证报告
   - 性能基准测试报告

3. **发布交付**
   - Windows 安装包 (.msi)
   - 便携版 (.zip)
   - 发布说明

4. **文档交付**
   - 扩展开发指南
   - 性能优化总结

---

## 6. 接口汇总

### 6.1 核心接口

| 接口名 | 职责 | 关键方法 |
|-------|------|---------|
| IFileLancetParser | 文件解析器 | CanParse, Parse, ParseAsync |
| IContentLoader | 内容加载 | LoadContentAsync, LoadTextAsync |
| IPreviewService | 预览服务 | GetPreviewAsync |

### 6.2 数据模型

| 类名 | 职责 | 关键属性 |
|-----|------|---------|
| FileNode | 文件树节点 | Name, Path, Type, Children |
| FileDetails | 文件详情 | Title, Authors, FileSize, Metadata |
| ParseResult | 解析结果 | Success, RootNode, Details |

### 6.3 视图模型

| 类名 | 职责 | 关键属性 |
|-----|------|---------|
| MainViewModel | 主窗口 VM | FileTreeNodes, SelectedNode, Commands |
| NodeDetailsViewModel | 详情面板 VM | Properties, Metadata |
| PreviewViewModel | 预览面板 VM | PreviewType, Content |

---

## 7. 开发规范

### 7.1 命名规范

- **类名**: PascalCase (如 `FileNode`, `EpubParser`)
- **接口名**: PascalCase + I 前缀 (如 `IFileLancetParser`)
- **方法名**: PascalCase (如 `ParseAsync`, `LoadContent`)
- **属性名**: PascalCase (如 `FileSize`, `SelectedNode`)
- **字段名**: _camelCase (如 `_selectedNode`, `_entryCache`)
- **常量名**: UPPER_SNAKE_CASE

### 7.2 异步规范

- 所有 IO 操作必须使用异步方法
- 异步方法名以 Async 结尾
- 使用 CancellationToken 支持取消
- 避免 async void

### 7.3 异常处理

- 使用自定义异常类
- 异常信息需本地化
- 记录异常日志
- 不向用户暴露敏感信息

---

## 8. 附录

### 8.1 依赖库

| 库名 | 用途 | 版本 |
|-----|------|------|
| HtmlAgilityPack | HTML 解析 | 最新版 |
| Microsoft.Web.WebView2 | HTML 渲染 | 最新版 |
| ICSharpCode.AvalonEdit | 代码高亮 | 最新版 |
| xUnit | 单元测试 | 最新版 |
| Moq | Mock 框架 | 最新版 |

### 8.2 参考文档

- EPUB 3.2 规范
- WPF MVVM 最佳实践
- WebView2 开发文档
