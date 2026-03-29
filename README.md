# File Lancet

一款面向开发者和高级用户的文件结构分析工具，像"手术刀"一样精准解剖文件，展示其内部物理结构和逻辑内容。

## 版本信息

| 版本 | 发布日期 | 说明 |
|------|----------|------|
| v0.1.1 | 2026-03-29 | 通用文件支持（可打开任何格式文件） |
| v0.1.0 | 2026-03-26 | 初始版本（EPUB文件分析） |

## 功能特性

### v0.1.0 功能
- ✅ EPUB 2.0/3.0/3.1 文件解析
- ✅ 文件结构树形浏览
- ✅ 元数据提取（书名、作者、出版社、语言、ISBN等）
- ✅ 内容预览（HTML、CSS、JavaScript、XML、图片）
- ✅ 十六进制预览（二进制文件）
- ✅ 双栏式布局（文件树+详情 / 预览）
- ✅ 拖拽打开文件
- ✅ XML命名空间自动修复
- ✅ ZIP条目多策略查找

### v0.1.1 新增功能
- ✅ 通用文件支持（可打开任何格式文件）
- ✅ GenericFileParser 兜底解析器
- ✅ 文件扩展名、MIME类型显示
- ✅ 文件创建时间显示

## 系统要求

- Windows 10/11 (x64)
- .NET 8/9/10
- 4GB 内存（推荐）
- 100MB 磁盘空间

## 使用方法

1. 下载 `FileLancet-v0.1.1-win-x64.zip`
2. 解压到任意目录
3. 运行 `FileLancet.UI.exe`
4. 点击 "Open" 按钮或拖拽文件到窗口

## 项目结构

```
FileLancet/
├── src/                    # 源代码
│   ├── FileLancet.Core/    # 核心解析库
│   ├── FileLancet.UI/      # WPF界面
│   └── FileLancet.CLI/     # 命令行工具
├── tests/                  # 测试项目
├── docs/                   # 文档
└── README.md              # 本文件
```

## 文档

- [需求规格说明书](./docs/Requirements.md)
- [技术路线纲要](./docs/TechnicalSpecification.md)
- [详细设计文档](./docs/DetailedDesign.md)
- [测试报告](./docs/TestReport.md)
- [发布说明](./RELEASE_NOTES.md)

## 测试

- **单元测试**: 137个
- **代码覆盖率**: 约92%

## 许可证

MIT License
