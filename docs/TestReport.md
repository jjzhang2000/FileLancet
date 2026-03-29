# File Lancet 测试报告

## 文档信息

- **项目**: File Lancet
- **版本**: v0.2.0
- **日期**: 2026-03-29
- **测试框架**: xUnit

---

## 测试概览

### 当前状态 (v0.1.1)

| 指标 | 数值 |
|------|------|
| 测试总数 | 137 |
| 通过 | 137 |
| 失败 | 0 |
| 跳过 | 0 |
| 整体状态 | ✅ 通过 |
| 代码覆盖率 | ~92% |

### v0.2.0 目标状态

| 指标 | 目标 |
|------|------|
| 测试总数 | 202 (新增 65) |
| 代码覆盖率 | ~91% |

---

## v0.1.0 测试（129个）

### 阶段一：核心 EPUB 解析引擎（58个测试）

#### Models 层测试 (TC-101 ~ TC-106)

| 测试编号 | 测试场景 | 测试结果 |
|---------|---------|---------|
| TC-101 | FileNode 属性验证 | ✅ 通过 |
| TC-102 | FileNode 父子关系 | ✅ 通过 |
| TC-103 | FileNode 路径计算 | ✅ 通过 |
| TC-104 | FileDetails 元数据 | ✅ 通过 |
| TC-105 | ParseResult 状态 | ✅ 通过 |
| TC-106 | NodeType 枚举 | ✅ 通过 |

**覆盖率**: Models 层 ≥ 95% ✅

#### Services 层测试 (TC-107 ~ TC-120)

| 测试编号 | 测试场景 | 测试结果 |
|---------|---------|---------|
| TC-107 | 标准 EPUB 2.0 解析 | ✅ 通过 |
| TC-108 | CanParse 有效文件 | ✅ 通过 |
| TC-109 | CanParse 不存在文件 | ✅ 通过 |
| TC-110 | CanParse 非 EPUB 扩展名 | ✅ 通过 |
| TC-111 | CanParse 无效 ZIP | ✅ 通过 |
| TC-112 | CanParse 缺少 container.xml | ✅ 通过 |
| TC-113 | Parse 有效 EPUB | ✅ 通过 |
| TC-114 | Parse 提取元数据 | ✅ 通过 |
| TC-115 | Parse 无效 EPUB | ✅ 通过 |
| TC-116 | Parse 不存在文件 | ✅ 通过 |
| TC-117 | Parse 构建文件树 | ✅ 通过 |
| TC-118 | ParseAsync 有效 EPUB | ✅ 通过 |
| TC-119 | ParseAsync 取消操作 | ✅ 通过 |
| TC-120 | 文件计数统计 | ✅ 通过 |

**覆盖率**: Services 层 ≥ 90% ✅

#### Factories 层测试

| 测试编号 | 测试场景 | 测试结果 |
|---------|---------|---------|
| TC-116 | RegisterParser 添加解析器 | ✅ 通过 |
| TC-116 | RegisterParser null 参数 | ✅ 通过 |
| TC-116 | RegisterParser 多个解析器 | ✅ 通过 |
| TC-116 | GetParser 匹配解析器 | ✅ 通过 |
| TC-116 | GetParser 无匹配 | ✅ 通过 |
| TC-116 | GetParser 空路径 | ✅ 通过 |
| TC-116 | GetParser null 路径 | ✅ 通过 |
| TC-116 | GetAllParsers 返回只读列表 | ✅ 通过 |
| TC-116 | ClearParsers 清空 | ✅ 通过 |
| TC-116 | InitializeDefaults 注册默认 | ✅ 通过 |
| TC-116 | InitializeDefaults 清空现有 | ✅ 通过 |
| TC-116 | 线程安全 | ✅ 通过 |

**覆盖率**: Factories 层 ≥ 90% ✅

### 阶段二：WPF 界面（15个测试）

#### ViewModels 层测试 (TC-201 ~ TC-214)

| 测试编号 | 测试场景 | 测试结果 |
|---------|---------|---------|
| TC-201 | MainViewModel 初始化 | ✅ 通过 |
| TC-202 | SelectedNode 设置 | ✅ 通过 |
| TC-203 | LoadFileCommand 执行 | ✅ 通过 |
| TC-204 | RefreshCommand CanExecute | ✅ 通过 |
| TC-205 | NodeDetails 更新 | ✅ 通过 |
| TC-206 | Preview 更新 | ✅ 通过 |
| TC-207 | IsLoading 设置 | ✅ 通过 |
| TC-208 | 错误处理 | ✅ 通过 |
| TC-209 | 文件加载 | ✅ 通过 |
| TC-210 | 节点选择 | ✅ 通过 |
| TC-211 | 栏宽调整 | ✅ 通过 |
| TC-212 | 空状态 | ✅ 通过 |
| TC-213 | 大数据量 | ✅ 通过 |
| TC-214 | 属性变更通知 | ✅ 通过 |

**覆盖率**: ViewModels 层 ≥ 85% ✅

### 阶段三：内容预览与交互（16个测试）

#### 预览服务单元测试 (TC-301 ~ TC-316)

| 测试编号 | 测试场景 | 测试结果 |
|---------|---------|---------|
| TC-301 | PreviewService 初始化 | ✅ 通过 |
| TC-302 | HTML 预览生成 | ✅ 通过 |
| TC-303 | 图片预览生成 | ✅ 通过 |
| TC-304 | 文本预览生成 | ✅ 通过 |
| TC-305 | 二进制预览生成 | ✅ 通过 |
| TC-306 | 预览内容加载 | ✅ 通过 |
| TC-307 | 预览缓存 | ✅ 通过 |
| TC-308 | 文件夹预览 | ✅ 通过 |
| TC-309 | JavaScript 预览 | ✅ 通过 |
| TC-310 | XML 预览 | ✅ 通过 |
| TC-311 | 空节点处理 | ✅ 通过 |
| TC-312 | 空加载器处理 | ✅ 通过 |
| TC-313 | 内容加载器编码检测 | ✅ 通过 |
| TC-314 | 内容存在检查 | ✅ 通过 |
| TC-315 | 缓存清除 | ✅ 通过 |
| TC-316 | 预览服务支持类型 | ✅ 通过 |

**覆盖率**: 预览服务层 ≥ 80% ✅

### 阶段四：扩展性验证与优化（16个测试）

#### BaseParser 单元测试 (TC-401 ~ TC-408)

| 测试编号 | 测试场景 | 测试结果 |
|---------|---------|---------|
| TC-401 | BaseParser 异常处理 | ✅ 通过 |
| TC-402 | BaseParser 节点创建 | ✅ 通过 |
| TC-403 | PlainTextParser CanParse | ✅ 通过 |
| TC-404 | PlainTextParser CanParse 非txt | ✅ 通过 |
| TC-405 | PlainTextParser Parse 有效txt | ✅ 通过 |
| TC-406 | PlainTextParser Parse 无效路径 | ✅ 通过 |
| TC-407 | 工厂注册新解析器 | ✅ 通过 |
| TC-408 | 工厂获取解析器 | ✅ 通过 |

#### 性能与扩展性测试 (TC-409 ~ TC-416)

| 测试编号 | 测试场景 | 测试结果 |
|---------|---------|---------|
| TC-409 | 扩展性验证 | ✅ 通过 |
| TC-410 | 内存泄漏 | ✅ 通过 |
| TC-411 | 并发性能 | ✅ 通过 |
| TC-412 | 缓存性能 | ✅ 通过 |
| TC-413 | 大文件处理 | ✅ 通过 |
| TC-414 | 进度报告 | ✅ 通过 |
| TC-415 | 缓存 LRU 策略 | ✅ 通过 |
| TC-416 | 内存监控 | ✅ 通过 |

**覆盖率**: BaseParser 层 ≥ 90% ✅

### 阶段五：XML 解析工具类（16个测试）

#### XmlParserHelper 单元测试 (TC-501 ~ TC-516)

| 测试编号 | 测试场景 | 测试结果 |
|---------|---------|---------|
| TC-501 | ParseWithNamespaceFix - 正常 XML | ✅ 通过 |
| TC-502 | ParseWithNamespaceFix - 未声明 dc 前缀 | ✅ 通过 |
| TC-503 | ParseWithNamespaceFix - 已声明 dc 前缀 | ✅ 通过 |
| TC-504 | ParseWithNamespaceFix - 空内容 | ✅ 通过 |
| TC-505 | GetZipEntry - 正斜杠路径 | ✅ 通过 |
| TC-506 | GetZipEntry - 反斜杠路径 | ✅ 通过 |
| TC-507 | GetZipEntry - 不区分大小写 | ✅ 通过 |
| TC-508 | GetZipEntry - 不存在路径 | ✅ 通过 |
| TC-509 | GetElementsByLocalName - 多个元素 | ✅ 通过 |
| TC-510 | GetFirstElementByLocalName - 第一个 | ✅ 通过 |
| TC-511 | GetFirstElementByLocalName - 不存在 | ✅ 通过 |
| TC-512 | GetElementValueByLocalName - 获取值 | ✅ 通过 |
| TC-513 | GetElementValueByLocalName - 不存在 | ✅ 通过 |
| TC-514 | LoadFromZipEntryAsync - 正常加载 | ✅ 通过 |
| TC-515 | LoadFromZipEntryAsync - 未声明命名空间 | ✅ 通过 |
| TC-516 | LoadFromZipEntryAsync - 取消操作 | ✅ 通过 |

**覆盖率**: XmlParserHelper 层 ≥ 90% ✅

### 阶段六：十六进制预览功能（8个测试）

#### 十六进制预览单元测试 (TC-601 ~ TC-608)

| 测试编号 | 测试场景 | 测试结果 |
|---------|---------|---------|
| TC-601 | FormatHexContent - 正常数据 | ✅ 通过 |
| TC-602 | FormatHexContent - 空数据 | ✅ 通过 |
| TC-603 | FormatHexContent - 偏移量格式 | ✅ 通过 |
| TC-604 | FormatHexContent - 每行字节数 | ✅ 通过 |
| TC-605 | FormatHexContent - ASCII对齐 | ✅ 通过 |
| TC-606 | FormatHexContent - 分隔线长度 | ✅ 通过 |
| TC-607 | PreviewViewModel - HexContent属性 | ✅ 通过 |
| TC-608 | PreviewViewModel - ShowHexView属性 | ✅ 通过 |

**覆盖率**: 十六进制预览层 ≥ 85% ✅

---

## v0.1.1 测试（8个）

### 阶段七：通用文件支持测试

#### GenericFileParser 单元测试 (TC-701 ~ TC-708)

| 测试编号 | 测试场景 | 测试目的 | 测试结果 |
|---------|---------|---------|---------|
| TC-701 | CanParse - 存在的文件 | 验证能解析存在的文件 | ✅ 通过 |
| TC-702 | CanParse - 不存在的文件 | 验证不能解析不存在的文件 | ✅ 通过 |
| TC-703 | CanParse - null路径 | 验证null路径处理 | ✅ 通过 |
| TC-704 | CanParse - 空路径 | 验证空路径处理 | ✅ 通过 |
| TC-705 | Parse - 存在的文件 | 验证解析成功 | ✅ 通过 |
| TC-706 | Parse - 不存在的文件 | 验证解析失败处理 | ✅ 通过 |
| TC-707 | Parse - 返回正确的文件详情 | 验证文件详情正确性 | ✅ 通过 |
| TC-708 | Parse - 根节点无子节点 | 验证文件树简化 | ✅ 通过 |

**覆盖率**: GenericFileParser ≥ 90% ✅

---

## v0.2.0 测试计划（65个）

### 阶段八：PDF 解析引擎（20个测试）

#### PdfParser 解析器测试 (TC-801 ~ TC-820)

| 测试编号 | 测试场景 | 测试内容 | 优先级 | 状态 |
|---------|---------|---------|--------|------|
| TC-801 | PdfParser CanParse 有效PDF | 验证 .pdf 文件识别 | 高 | 📝 待开发 |
| TC-802 | PdfParser CanParse 非PDF | 验证非 PDF 文件返回 false | 高 | 📝 待开发 |
| TC-803 | PdfParser CanParse 空路径 | 验证空路径处理 | 高 | 📝 待开发 |
| TC-804 | PdfParser CanParse null路径 | 验证 null 路径处理 | 高 | 📝 待开发 |
| TC-805 | PdfParser CanParse 不存在文件 | 验证不存在文件处理 | 高 | 📝 待开发 |
| TC-806 | PdfParser Parse 标准PDF | 验证标准 PDF 解析成功 | 高 | 📝 待开发 |
| TC-807 | PdfParser Parse 提取元数据 | 验证标题、作者、页数等提取 | 高 | 📝 待开发 |
| TC-808 | PdfParser Parse 多作者PDF | 验证多作者提取 | 中 | 📝 待开发 |
| TC-809 | PdfParser Parse 中文PDF | 验证中文元数据提取 | 中 | 📝 待开发 |
| TC-810 | PdfParser Parse 加密PDF | 验证加密 PDF 检测 | 高 | 📝 待开发 |
| TC-811 | PdfParser Parse 损坏PDF | 验证损坏文件错误处理 | 高 | 📝 待开发 |
| TC-812 | PdfParser Parse 不存在文件 | 验证不存在文件错误处理 | 高 | 📝 待开发 |
| TC-813 | PdfParser Parse 空PDF | 验证空 PDF 文件处理 | 中 | 📝 待开发 |
| TC-814 | PdfParser 构建文件树 | 验证页面节点生成 | 高 | 📝 待开发 |
| TC-815 | PdfParser 单页PDF | 验证单页 PDF 文件树 | 中 | 📝 待开发 |
| TC-816 | PdfParser 多页PDF | 验证多页 PDF 文件树（100+页） | 中 | 📝 待开发 |
| TC-817 | PdfParser 大纲提取 | 验证书签/大纲提取 | 中 | 📝 待开发 |
| TC-818 | PdfParser 嵌套大纲 | 验证嵌套书签提取 | 中 | 📝 待开发 |
| TC-819 | PdfParser 无大纲PDF | 验证无书签 PDF 处理 | 中 | 📝 待开发 |
| TC-820 | PdfParser ParseAsync 异步 | 验证异步解析 | 高 | 📝 待开发 |

**目标覆盖率**: PdfParser ≥ 90%

### 阶段九：PDF 页面渲染预览（15个测试）

#### PdfRenderService 渲染服务测试 (TC-821 ~ TC-835)

| 测试编号 | 测试场景 | 测试内容 | 优先级 | 状态 |
|---------|---------|---------|--------|------|
| TC-821 | RenderPage 第一页 | 验证首页渲染 | 高 | 📝 待开发 |
| TC-822 | RenderPage 中间页 | 验证中间页渲染 | 高 | 📝 待开发 |
| TC-823 | RenderPage 最后一页 | 验证末页渲染 | 高 | 📝 待开发 |
| TC-824 | RenderPage 无效页码 | 验证无效页码处理 | 高 | 📝 待开发 |
| TC-825 | RenderPage 缩放1x | 验证实际大小渲染 | 中 | 📝 待开发 |
| TC-826 | RenderPage 缩放2x | 验证2倍放大渲染 | 中 | 📝 待开发 |
| TC-827 | RenderPage 缩放0.5x | 验证缩小渲染 | 中 | 📝 待开发 |
| TC-828 | RenderThumbnail 默认尺寸 | 验证默认缩略图 | 中 | 📝 待开发 |
| TC-829 | RenderThumbnail 自定义尺寸 | 验证自定义尺寸缩略图 | 中 | 📝 待开发 |
| TC-830 | ExtractText 纯文本PDF | 验证纯文本提取 | 高 | 📝 待开发 |
| TC-831 | ExtractText 图文混排PDF | 验证图文混排文本提取 | 中 | 📝 待开发 |
| TC-832 | ExtractText 扫描PDF | 验证扫描件文本提取（OCR） | 低 | 📝 待开发 |
| TC-833 | GetPageSize | 验证页面尺寸获取 | 中 | 📝 待开发 |
| TC-834 | RenderPage 取消令牌 | 验证取消操作 | 中 | 📝 待开发 |
| TC-835 | RenderPage 并发渲染 | 验证多页面并发渲染 | 低 | 📝 待开发 |

**目标覆盖率**: PdfRenderService ≥ 85%

### 阶段十：PDF 预览视图模型（15个测试）

#### PdfPreviewViewModel 视图模型测试 (TC-836 ~ TC-850)

| 测试编号 | 测试场景 | 测试内容 | 优先级 | 状态 |
|---------|---------|---------|--------|------|
| TC-836 | 初始化VM | 验证初始状态 | 高 | 📝 待开发 |
| TC-837 | 加载PDF | 验证 PDF 加载 | 高 | 📝 待开发 |
| TC-838 | 当前页变更 | 验证页面切换 | 高 | 📝 待开发 |
| TC-839 | PreviousPageCommand | 验证上一页命令 | 高 | 📝 待开发 |
| TC-840 | NextPageCommand | 验证下一页命令 | 高 | 📝 待开发 |
| TC-841 | FirstPageCommand | 验证首页命令 | 高 | 📝 待开发 |
| TC-842 | LastPageCommand | 验证末页命令 | 高 | 📝 待开发 |
| TC-843 | ZoomInCommand | 验证放大命令 | 中 | 📝 待开发 |
| TC-844 | ZoomOutCommand | 验证缩小命令 | 中 | 📝 待开发 |
| TC-845 | FitWidthCommand | 验证适应宽度 | 中 | 📝 待开发 |
| TC-846 | FitHeightCommand | 验证适应高度 | 中 | 📝 待开发 |
| TC-847 | ShowTextMode 切换 | 验证文本模式切换 | 中 | 📝 待开发 |
| TC-848 | 边界页码测试 | 验证页码边界（<1, >TotalPages） | 高 | 📝 待开发 |
| TC-849 | 属性变更通知 | 验证 INotifyPropertyChanged | 高 | 📝 待开发 |
| TC-850 | 异常处理 | 验证渲染异常处理 | 高 | 📝 待开发 |

**目标覆盖率**: PdfPreviewViewModel ≥ 85%

### 阶段十一：PDF 数据模型（5个测试）

#### PdfDetails 数据模型测试 (TC-851 ~ TC-855)

| 测试编号 | 测试场景 | 测试内容 | 优先级 | 状态 |
|---------|---------|---------|--------|------|
| TC-851 | PdfDetails 属性设置 | 验证所有属性可设置 | 高 | 📝 待开发 |
| TC-852 | PdfDetails 继承FileDetails | 验证继承关系 | 高 | 📝 待开发 |
| TC-853 | PdfPageInfo 属性验证 | 验证页面信息属性 | 中 | 📝 待开发 |
| TC-854 | PdfDetails Pages列表 | 验证页面列表操作 | 中 | 📝 待开发 |
| TC-855 | PdfDetails 空值处理 | 验证空值安全 | 中 | 📝 待开发 |

**目标覆盖率**: PdfDetails ≥ 95%

### 阶段十二：PDF 集成测试（10个测试）

#### 集成测试 (TC-856 ~ TC-865)

| 测试编号 | 测试场景 | 测试内容 | 优先级 | 状态 |
|---------|---------|---------|--------|------|
| TC-856 | 完整PDF解析流程 | 端到端解析测试 | 高 | 📝 待开发 |
| TC-857 | PDF 1.4版本 | 验证 PDF 1.4 兼容性 | 中 | 📝 待开发 |
| TC-858 | PDF 1.7版本 | 验证 PDF 1.7 兼容性 | 中 | 📝 待开发 |
| TC-859 | PDF/A标准 | 验证 PDF/A 兼容性 | 低 | 📝 待开发 |
| TC-860 | 大文件PDF | 验证 100MB+ PDF 处理 | 中 | 📝 待开发 |
| TC-861 | 多线程安全 | 验证多线程解析安全 | 中 | 📝 待开发 |
| TC-862 | 内存泄漏 | 验证无内存泄漏 | 高 | 📝 待开发 |
| TC-863 | 渲染性能 | 验证渲染时间 < 500ms | 高 | 📝 待开发 |
| TC-864 | 快速翻页 | 验证快速翻页响应 | 中 | 📝 待开发 |
| TC-865 | 工厂注册 | 验证 PdfParser 工厂注册 | 高 | 📝 待开发 |

**目标覆盖率**: 集成测试 ≥ 80%

---

## 测试覆盖率汇总

### 当前状态 (v0.1.1)

| 模块 | 目标覆盖率 | 实际覆盖率 | 状态 |
|------|-----------|-----------|------|
| Models | ≥ 95% | ~98% | ✅ 达标 |
| Services | ≥ 90% | ~92% | ✅ 达标 |
| Factories | ≥ 90% | ~95% | ✅ 达标 |
| ViewModels | ≥ 85% | ~88% | ✅ 达标 |
| PreviewService | ≥ 80% | ~85% | ✅ 达标 |
| BaseParser | ≥ 90% | ~92% | ✅ 达标 |
| XmlParserHelper | ≥ 90% | ~93% | ✅ 达标 |
| HexPreview | ≥ 85% | ~88% | ✅ 达标 |
| GenericFileParser | ≥ 90% | ~93% | ✅ 达标 |
| **整体** | **≥ 90%** | **~92%** | ✅ **达标** |

### v0.2.0 目标

| 模块 | 目标覆盖率 | 预计实际 | 状态 |
|------|-----------|-----------|------|
| PdfParser | ≥ 90% | ~92% | 📝 计划中 |
| PdfRenderService | ≥ 85% | ~88% | 📝 计划中 |
| PdfPreviewViewModel | ≥ 85% | ~87% | 📝 计划中 |
| PdfDetails | ≥ 95% | ~97% | 📝 计划中 |
| **整体** | **≥ 90%** | **~91%** | 📝 **计划中** |

---

## 关键验证点

### v0.1.0 验证点 (已完成)

- ✅ 能正确识别 EPUB 文件格式
- ✅ 能正确解析 container.xml 获取 OPF 路径
- ✅ 能正确解析 OPF 文件提取元数据
- ✅ 能正确处理 XML 命名空间
- ✅ 能正确构建文件树结构
- ✅ 对不存在文件返回友好错误
- ✅ 对损坏文件返回明确错误信息
- ✅ 支持动态注册解析器
- ✅ 左栏文件树正确显示文件结构
- ✅ 中栏详情面板动态显示节点信息
- ✅ 右栏预览面板根据文件类型显示内容
- ✅ HTML 内容预览功能
- ✅ 图片内容预览功能
- ✅ 文本/代码预览功能
- ✅ 二进制文件十六进制预览功能
- ✅ BaseParser 抽象基类实现
- ✅ PlainTextParser 示例解析器实现
- ✅ XmlParserHelper 工具类实现

### v0.1.1 验证点 (已完成)

- ✅ GenericFileParser 通用解析器实现
- ✅ 支持打开任何格式的文件
- ✅ 不支持的格式默认以十六进制方式预览
- ✅ 文件树只保留文件名（无子节点结构）
- ✅ 详情面板显示扩展名、MIME类型、创建时间
- ✅ FileDetails 模型新增属性正确

### v0.2.0 验证点 (计划中)

- 📝 PdfParser 解析器实现
- 📝 PDF 元数据提取（标题、作者、页数等）
- 📝 PDF 文件树构建（页面、大纲）
- 📝 PDF 页面渲染预览
- 📝 PDF 分页导航（上一页/下一页）
- 📝 PDF 缩放控制（放大/缩小/适应）
- 📝 PDF 文本提取预览
- 📝 PdfDetails 数据模型扩展
- 📝 加密 PDF 检测
- 📝 多版本 PDF 兼容性

---

## 测试执行命令

```bash
# 运行所有测试
dotnet test

# 运行特定模块测试
dotnet test --filter "Models"
dotnet test --filter "Services"
dotnet test --filter "GenericFileParser"
dotnet test --filter "PdfParser"           # v0.2.0 新增
dotnet test --filter "PdfRenderService"    # v0.2.0 新增

# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"

# 运行特定版本测试
dotnet test --filter "TC-8xx"              # PDF 相关测试
```

---

## 测试文件准备

### v0.2.0 PDF 测试文件

| 文件名 | 用途 | 说明 |
|--------|------|------|
| test_standard.pdf | 标准测试 | 包含完整元数据、多页面 |
| test_single_page.pdf | 单页测试 | 仅包含1页 |
| test_multi_page.pdf | 多页测试 | 包含100+页 |
| test_encrypted.pdf | 加密测试 | 需要密码的PDF |
| test_chinese.pdf | 中文测试 | 包含中文元数据 |
| test_bookmarks.pdf | 大纲测试 | 包含书签/大纲 |
| test_corrupted.pdf | 损坏测试 | 损坏的PDF文件 |
| test_1.4.pdf | 版本测试 | PDF 1.4版本 |
| test_1.7.pdf | 版本测试 | PDF 1.7版本 |
| test_large.pdf | 性能测试 | 100MB+大文件 |

---

## 结论

### v0.1.0 完成状态: ✅ 已完成

核心功能全部实现并通过测试，包括 EPUB 解析引擎、WPF 界面、内容预览、扩展性架构、XML 工具类和十六进制预览功能。

### v0.1.1 完成状态: ✅ 已完成

通用文件支持功能实现并通过测试，应用现在可以打开任何格式的文件。

### v0.2.0 计划状态: 📝 计划中

PDF 支持功能正在规划中，预计新增 65 个单元测试，目标代码覆盖率 ≥ 90%。

### 整体项目状态

| 版本 | 测试数量 | 状态 |
|------|---------|------|
| v0.1.0 | 129 | ✅ 已完成 |
| v0.1.1 | 8 | ✅ 已完成 |
| v0.2.0 | 65 | 📝 计划中 |
| **总计** | **202** | **进行中** |

**当前：137个单元测试全部通过，代码覆盖率约92%**
**目标：202个单元测试，代码覆盖率约91%**
