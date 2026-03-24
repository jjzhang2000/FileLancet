# File Lancet

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia%20UI-11.0-8B44AC)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**File Lancet** 是一款面向开发者和高级用户的文件结构分析与验证工具。它能够像"手术刀"一样精准地解剖 EPUB 电子书文件，展示其内部的物理结构和逻辑内容。

![File Lancet Screenshot](docs/screenshot.png)

## 功能特性

- **精准解析**：深度解析 EPUB 文件，还原目录结构、元数据、阅读顺序及具体内容
- **三栏式界面**：经典的左-中-右三栏布局，展示文件层级、属性详情和内容预览
- **智能分类**：自动将文件归类为"正文"、"样式"、"图片"、"配置"等逻辑文件夹
- **元数据提取**：提取书名、作者、出版社、语言、ISBN 等书籍信息
- **跨平台支持**：基于 Avalonia UI，完美支持 Windows 和 Linux
- **插件化架构**：预留扩展接口，未来可支持 PDF、MOBI 等其他格式

## 技术栈

- **开发语言**：C# (.NET 8)
- **UI 框架**：Avalonia UI 11.0
- **架构模式**：MVVM (Model-View-ViewModel)
- **依赖注入**：内置 DI 容器
- **测试框架**：xUnit + FluentAssertions + Moq

## 项目结构

```
FileLancet/
├── src/
│   ├── FileLancet.Core/          # 核心解析库
│   │   ├── Models/               # 数据模型
│   │   │   ├── FileNode.cs       # 文件节点
│   │   │   ├── FileDetails.cs    # 文件详情
│   │   │   ├── NodeType.cs       # 节点类型枚举
│   │   │   └── ParseResult.cs    # 解析结果
│   │   └── Services/             # 服务层
│   │       ├── IFileLancetParser.cs  # 解析器接口
│   │       ├── ParserFactory.cs      # 解析器工厂
│   │       └── EpubParser.cs         # EPUB 解析器
│   └── FileLancet.UI/            # Avalonia UI 应用
│       ├── ViewModels/           # 视图模型
│       │   ├── ViewModelBase.cs
│       │   └── MainViewModel.cs
│       ├── Views/                # 视图
│       │   ├── MainWindow.axaml
│       │   └── MainWindow.axaml.cs
│       ├── App.axaml
│       ├── App.axaml.cs
│       └── Program.cs
├── tests/
│   ├── FileLancet.Core.Tests/    # 核心库单元测试
│   └── FileLancet.UI.Tests/      # UI 测试
├── docs/                         # 文档
├── FileLancet.sln               # 解决方案文件
└── README.md                    # 本文件
```

## 快速开始

### 环境要求

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本
- Visual Studio 2022 / JetBrains Rider / VS Code

### 构建项目

```bash
# 克隆仓库
git clone https://github.com/yourusername/FileLancet.git
cd FileLancet

# 还原依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行应用
dotnet run --project src/FileLancet.UI
```

### 运行测试

```bash
# 运行所有测试
dotnet test

# 运行核心库测试
dotnet test tests/FileLancet.Core.Tests

# 生成测试覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

## 使用指南

### 打开 EPUB 文件

1. 启动应用后，点击菜单栏 **文件 -> 打开...**
2. 选择要分析的 `.epub` 文件
3. 或使用拖拽功能，将 EPUB 文件拖入应用窗口

### 界面说明

- **左栏**：文件结构树，展示 EPUB 包内的所有文件和文件夹
- **中栏**：属性详情，显示选中文件的元数据和物理属性
- **右栏**：内容预览（开发中）
- **底部状态栏**：显示当前操作状态和文件路径

### 支持的文件格式

| 格式 | 状态 | 说明 |
|------|------|------|
| EPUB 2.0/3.0/3.1 | ✅ 已支持 | 完整的元数据和结构解析 |
| PDF | 🚧 计划中 | 预留接口，待实现 |
| MOBI | 🚧 计划中 | 预留接口，待实现 |
| TXT | 🚧 计划中 | 作为扩展性验证格式 |

## 开发计划

### 阶段一：核心地基 ✅
- [x] 搭建 Avalonia UI 项目结构
- [x] 实现 IFileLancetParser 接口定义
- [x] 完成 EpubParser 基础实现（解包 + 读取 OPF）
- [x] 定义数据模型（FileNode、FileDetails、NodeType）

### 阶段二：界面实现 🚧
- [x] 实现三栏式布局
- [x] 完成左栏树形结构绑定
- [x] 完成中栏属性动态展示
- [ ] 实现右栏预览功能（HTML 渲染）
- [ ] 完善底部状态栏交互
- [ ] 增加拖拽打开文件功能

### 阶段三：细节完善
- [ ] 实现代码高亮显示
- [ ] 添加异常处理和用户提示
- [ ] 优化大文件加载性能
- [ ] 添加文件关联支持

### 阶段四：扩展与优化
- [ ] 实现 PDF 解析器
- [ ] 跨平台打包（Windows/Linux）
- [ ] 性能基准测试
- [ ] 发布 v1.0 正式版

## 贡献指南

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

### 代码规范

- 遵循 C# 命名规范（PascalCase/camelCase）
- 文件 I/O 操作必须使用异步方法（async/await）
- 核心业务逻辑代码覆盖率不低于 80%
- 提交前运行 `dotnet format` 格式化代码

## 相关文档

- [需求说明书](Requirements.md) - 项目需求详细说明
- [技术路线纲要](Technical%20Specification.md) - 技术架构和开发规划

## 许可证

本项目采用 [MIT](LICENSE) 许可证开源。

## 致谢

- [Avalonia UI](https://avaloniaui.net/) - 跨平台 UI 框架
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) - MVVM 工具包
- [HtmlAgilityPack](https://html-agility-pack.net/) - HTML 解析库（可选）

---

<p align="center">
  Made with ❤️ by File Lancet Team
</p>
