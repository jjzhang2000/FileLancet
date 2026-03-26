# File Lancet - 文件分析工具

[![.NET](https://img.shields.io/badge/.NET-8.0%2B-blue)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> 像手术刀一样精准解剖文件结构

File Lancet 是一款面向开发者和高级用户的文件结构分析工具，专注于 EPUB 电子书格式的深度解析。它提供直观的三栏式界面，展示文件的物理结构、元数据详情和内容预览。

![版本](https://img.shields.io/badge/Version-0.1-orange)

## 功能特性

### 核心功能
- **EPUB 文件解析**：完整解析 EPUB 2.0/3.0/3.1 格式
- **文件结构浏览**：树形展示 ZIP 容器内的所有文件和文件夹
- **元数据提取**：书名、作者、出版社、语言、ISBN、版本等信息
- **内容预览**：支持 HTML、CSS、JavaScript、XML、图片、二进制文件的预览
- **十六进制查看**：二进制文件以十六进制格式显示，支持 32 字节/行

### 交互特性
- **多方式打开**：文件对话框、拖拽加载、文件关联
- **三栏式布局**：文件树（左上）、详情面板（左下）、内容预览（右侧）
- **自适应调整**：支持拖拽调整各栏宽度
- **实时预览**：选择文件节点即时显示内容和预览

### 技术特性
- **懒加载**：大文件按需加载，优化内存使用
- **缓存机制**：内容缓存避免重复读取
- **异步处理**：解析和加载不阻塞 UI
- **扩展架构**：插件化设计，支持添加新的文件格式解析器

## 界面预览

```
┌─────────────────────────────────────────────────────────────────────────┐
│  [Open] [About]                                                         │
├──────────────────────┬──────────────────────────────────────────────────┤
│                      │                                                  │
│  File Structure      │                                                  │
│  ├─ META-INF         │                                                  │
│  │  └─ container.xml │                                                  │
│  ├─ OEBPS            │                                                  │
│  │  ├─ content.opf   │                                                  │
│  │  ├─ chapter1.html │              Preview: chapter1.html              │
│  │  └─ styles.css    │  ┌──────────────────────────────────────────┐   │
│  └─ mimetype         │  │ Offset  00 01 02 03 ... 1F  ASCII        │   │
│                      │  │ 000000  50 4B 03 04 ...     PK...        │   │
├──────────────────────┤  │ ...                                      │   │
│                      │  └──────────────────────────────────────────┘   │
│  Details             │                                                  │
│  ─────────────────   │                                                  │
│  Name: content.opf   │                                                  │
│  Type: Opf           │                                                  │
│  Size: 2.5 KB        │                                                  │
│                      │                                                  │
│  EPUB Metadata       │                                                  │
│  ─────────────────   │                                                  │
│  Title: Example Book │                                                  │
│  Authors: John Doe   │                                                  │
│  ...                 │                                                  │
├──────────────────────┴──────────────────────────────────────────────────┤
│  Ready | D:\Books\example.epub                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

## 系统要求

- **操作系统**：Windows 10/11 (x64)
- **运行时**：.NET 8.0 或更高版本
- **内存**：建议 4GB 及以上
- **磁盘空间**：100MB 可用空间

## 快速开始

### 安装

1. 克隆仓库
```bash
git clone https://github.com/yourusername/filelancet.git
cd filelancet
```

2. 构建项目
```bash
dotnet build
```

3. 运行应用
```bash
dotnet run --project src/FileLancet.UI
```

### 使用

1. 点击 **Open** 按钮或拖拽 EPUB 文件到窗口
2. 在左侧文件树中浏览文件结构
3. 点击节点查看详情（左下）和内容预览（右侧）
4. 支持预览的文件类型：
   - HTML/XHTML：树状结构显示
   - CSS/JS/XML：代码高亮显示
   - 图片：直接显示
   - 二进制文件：十六进制视图

## 项目结构

```
FileLancet/
├── src/
│   ├── FileLancet.Core/          # 核心解析库
│   │   ├── Interfaces/            # 接口定义
│   │   ├── Models/                # 数据模型
│   │   ├── Services/              # 解析服务
│   │   ├── Utilities/             # 工具类
│   │   └── Factories/             # 工厂类
│   └── FileLancet.UI/             # WPF 应用程序
│       ├── Views/                 # XAML 视图
│       ├── ViewModels/            # 视图模型
│       └── Converters/            # 值转换器
├── tests/                         # 单元测试
│   ├── FileLancet.Core.Tests/
│   └── FileLancet.UI.Tests/
└── docs/                          # 文档
    ├── Requirements.md
    ├── DetailedDesign.md
    ├── TechnicalSpecification.md
    └── TestReport.md
```

## 技术栈

- **开发语言**：C# (.NET 8)
- **UI 框架**：WPF (Windows Presentation Foundation)
- **架构模式**：MVVM (Model-View-ViewModel)
- **测试框架**：xUnit
- **核心库**：
  - `System.IO.Compression` - ZIP 文件处理
  - `System.Xml.Linq` - XML 解析

## 开发阶段

| 阶段 | 内容 | 状态 | 测试 |
|------|------|------|------|
| 阶段一 | 核心 EPUB 解析引擎 | ✅ 完成 | 58 个测试 |
| 阶段二 | WPF 三栏式界面 | ✅ 完成 | 15 个测试 |
| 阶段三 | 内容预览与交互 | ✅ 完成 | 16 个测试 |
| 阶段四 | 扩展性与优化 | ✅ 完成 | 16 个测试 |
| 阶段五 | XML 解析工具类 | ✅ 完成 | 16 个测试 |
| 阶段六 | 十六进制预览 | ✅ 完成 | 8 个测试 |

**总计：129 个单元测试，代码覆盖率约 92%**

## 验证脚本

运行 `FileLancet.bat` 执行完整验证：

```bash
FileLancet.bat
```

验证内容包括：
- .NET SDK 检查
- 项目构建
- 单元测试执行
- 各阶段功能验证

## 扩展开发

### 添加新的文件格式支持

1. 实现 `IFileLancetParser` 接口
2. 继承 `BaseParser` 抽象基类
3. 注册到 `ParserFactory`

示例：
```csharp
public class MyParser : BaseParser
{
    public override bool CanParse(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".myext", StringComparison.OrdinalIgnoreCase);
    }
    
    public override ParseResult Parse(string filePath)
    {
        // 实现解析逻辑
    }
}

// 注册解析器
ParserFactory.RegisterParser(new MyParser());
```

## 文档

- [需求说明书](docs/Requirements.md)
- [详细设计文档](docs/DetailedDesign.md)
- [技术规格文档](docs/TechnicalSpecification.md)
- [测试报告](docs/TestReport.md)

## 贡献

欢迎提交 Issue 和 Pull Request！

## 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 致谢

- 感谢 .NET 社区提供的优秀工具和库
- 感谢 EPUB 规范制定者提供的开放标准

---

**注意**：File Lancet 目前处于 0.1 版本，功能仍在持续完善中。
