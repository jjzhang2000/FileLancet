namespace FileLancet.Core.Models;

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
    Other,          // 其他文件
    
    // PDF 相关类型 (v0.2.0 新增)
    PdfDocument,    // PDF 文档根节点
    PdfPage,        // PDF 页面
    PdfOutline,     // PDF 书签/大纲
    PdfFont,        // PDF 字体
    PdfImage        // PDF 内嵌图片
}
