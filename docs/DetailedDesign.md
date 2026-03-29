# File Lancet 详细设计文档

## 文档信息

- **项目**: File Lancet
- **版本**: v0.1.1
- **日期**: 2026-03-29
- **状态**: 已完成

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

## 4. 界面布局设计

### 4.1 双栏布局

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

---

## 5. 测试设计

### 5.1 单元测试覆盖率目标

| 模块 | 覆盖率目标 | 实际覆盖率 |
|------|-----------|-----------|
| Models | ≥ 95% | ~98% |
| Services | ≥ 90% | ~92% |
| Factories | ≥ 90% | ~95% |
| ViewModels | ≥ 85% | ~88% |
| Utilities | ≥ 85% | ~93% |
| **整体** | **≥ 90%** | **~92%** |

### 5.2 测试用例分布

| 版本 | 测试数量 | 主要内容 |
|------|---------|---------|
| v0.1.0 | 129 | 核心解析、界面、预览、扩展性、XML工具、十六进制 |
| v0.1.1 | 8 | 通用文件解析器 |
| **总计** | **137** | - |

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
| FileDetails | 文件详情 | Title, Authors, FileSize, Metadata, FileExtension, MimeType, CreatedTime |
| ParseResult | 解析结果 | Success, RootNode, Details |

### 6.3 视图模型

| 类名 | 职责 | 关键属性 |
|-----|------|---------|
| MainViewModel | 主窗口 VM | FileTreeNodes, SelectedNode, Commands |
| NodeDetailsViewModel | 详情面板 VM | Properties, Metadata |
| PreviewViewModel | 预览面板 VM | PreviewType, Content, HexContent, ShowHexView |

### 6.4 工具类

| 类名 | 职责 | 关键方法 |
|-----|------|---------|
| XmlParserHelper | XML 解析辅助 | ParseWithNamespaceFix, GetZipEntry, GetElementValueByLocalName |
| PerformanceOptimizer | 性能优化 | CreateCache, MonitorMemory, ThrottleAsync |
