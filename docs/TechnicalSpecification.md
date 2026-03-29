# File Lancet 技术路线纲要

## 文档信息

- **项目**: File Lancet
- **版本**: v0.1.1
- **日期**: 2026-03-29

---

## 一、核心技术栈选型

### 开发语言
- **C# (.NET 8 / .NET 9)**
  - 理由：利用 .NET 强大的原生库（System.IO.Compression, System.Xml.Linq）处理文件底层结构；强类型系统确保数据模型的严谨性；跨平台编译能力成熟。

### UI 框架
- **WPF (Windows Presentation Foundation)**
  - 理由：Windows 原生 UI 框架，与 Windows 系统深度集成；提供丰富的控件库（TreeView、DataGrid、WebView2 等）和成熟的数据绑定机制；基于 DirectX 的硬件加速渲染，性能优异。

### 架构模式
- **MVVM**
  - 理由：实现 UI 与业务逻辑的彻底解耦，便于单元测试核心解析功能。

### 开发工具
- Visual Studio 2026

---

## 二、系统架构设计

### 分层架构
- **表现层**：负责界面展示与用户交互（WPF Views/XAML）。
- **视图模型层**：负责状态管理与数据绑定（ViewModels）。
- **业务逻辑层**：核心解析引擎，负责调度不同的文件解析器（Services）。
- **数据模型层**：定义通用的文件节点结构与解析结果标准（Models）。

### 插件化解析策略
- **接口定义**：定义通用解析接口 `IFileLancetParser`，规定 `CanParse` 和 `Parse` 的标准行为。
- **适配器实现**：针对每种文件格式（EPUB, 纯文本等）实现独立的解析器类。
- **工厂分发**：核心引擎根据文件头或扩展名，自动匹配并实例化对应的解析器。

---

## 三、关键开发思路

### 数据驱动 UI
- 界面不直接操作文件，而是操作内存中的标准数据模型（FileNode树）。
- 解析器将物理文件转换为标准 JSON/对象结构，UI 仅负责渲染这些对象。

### 懒加载机制
- 针对大文件（如数百兆的 EPUB），在构建树形结构时不立即读取文件内容。
- 仅在用户点击节点或请求预览时，才从磁盘/压缩包中读取具体字节流，以优化内存占用。

---

## 四、版本技术演进

### v0.1.0 技术实现

#### 阶段一：核心地基与 EPUB 解析引擎
- 构建通用数据模型（FileNode, FileDetails, ParseResult）
- 实现 EPUB 解析策略（解包、OPF解析、目录构建）
- 实现懒加载机制

#### 阶段二：双栏式界面实现
- 搭建 MVVM 框架
- 实现双栏布局（文件树+详情 / 预览）
- 完成数据绑定与交互

#### 阶段三：内容预览与交互完善
- 实现预览服务（HTML、图片、文本、二进制）
- 集成拖拽打开功能
- 完善异常处理

#### 阶段四：扩展性验证与优化
- 提取 BaseParser 抽象基类
- 实现 PlainTextParser 示例解析器
- 性能优化（内存缓存、并发控制）

#### 阶段五：XML 解析工具类
- 实现 XmlParserHelper 工具类
- 命名空间自动修复功能
- ZIP 条目多策略查找

#### 阶段六：十六进制预览与布局优化
- 实现十六进制格式化显示
- 优化双栏布局

### v0.1.1 技术实现

#### 阶段七：通用文件支持（兜底功能）
- 实现 GenericFileParser 通用解析器
- 作为兜底解析器最后注册
- 支持任何格式文件的基本信息显示
- 十六进制预览支持任意文件

---

## 五、接口汇总

### 核心接口

| 接口名 | 职责 | 关键方法 |
|-------|------|---------|
| IFileLancetParser | 文件解析器 | CanParse, Parse, ParseAsync |
| IContentLoader | 内容加载 | LoadContentAsync, LoadTextAsync |
| IPreviewService | 预览服务 | GetPreviewAsync |

### 数据模型

| 类名 | 职责 | 关键属性 |
|-----|------|---------|
| FileNode | 文件树节点 | Name, Path, Type, Children |
| FileDetails | 文件详情 | Title, Authors, FileSize, Metadata, FileExtension, MimeType |
| ParseResult | 解析结果 | Success, RootNode, Details |

### 视图模型

| 类名 | 职责 | 关键属性 |
|-----|------|---------|
| MainViewModel | 主窗口 VM | FileTreeNodes, SelectedNode, Commands |
| NodeDetailsViewModel | 详情面板 VM | Properties, Metadata |
| PreviewViewModel | 预览面板 VM | PreviewType, Content, HexContent, ShowHexView |

### 工具类

| 类名 | 职责 | 关键方法 |
|-----|------|---------|
| XmlParserHelper | XML 解析辅助 | ParseWithNamespaceFix, GetZipEntry, GetElementValueByLocalName |
| PerformanceOptimizer | 性能优化 | CreateCache, MonitorMemory, ThrottleAsync |

---

## 六、开发规范

### 命名规范
- **类名**: PascalCase (如 `FileNode`, `EpubParser`)
- **接口名**: PascalCase + I 前缀 (如 `IFileLancetParser`)
- **方法名**: PascalCase (如 `ParseAsync`, `LoadContent`)
- **属性名**: PascalCase (如 `FileSize`, `SelectedNode`)
- **字段名**: _camelCase (如 `_selectedNode`, `_entryCache`)
- **常量名**: UPPER_SNAKE_CASE

### 异步规范
- 所有 IO 操作必须使用异步方法
- 异步方法名以 Async 结尾
- 使用 CancellationToken 支持取消
- 避免 async void

### 异常处理
- 使用自定义异常类
- 异常信息需本地化
- 记录异常日志
- 不向用户暴露敏感信息

---

## 七、测试要求

### 单元测试
- **测试框架**：xUnit
- **覆盖率目标**：核心业务逻辑层不低于 80%
- **Mock 策略**：使用 Moq 模拟文件系统和外部依赖

### 集成测试
- 完整 EPUB 文件解析流程
- 多种 EPUB 版本兼容性测试（2.0、3.0、3.1）
- 异常文件处理（损坏的 ZIP、缺失关键文件）

### 性能测试
- 大文件（>100MB）加载时间不超过 5 秒
- 内存占用不超过文件大小的 3 倍
- 树形结构渲染 10000+ 节点时保持流畅

---

## 八、版本里程碑

### v0.1.0 里程碑（2026-03-26）
- ✅ 控制台级 EPUB 解析内核
- ✅ 双栏式 WPF 界面
- ✅ 内容预览功能（HTML、图片、代码）
- ✅ 扩展性架构验证
- ✅ XML 解析工具类
- ✅ 十六进制预览功能

### v0.1.1 里程碑（2026-03-29）
- ✅ 通用文件支持（兜底解析器）
- ✅ 支持打开任何格式文件
- ✅ 文件信息扩展（扩展名、MIME、创建时间）
