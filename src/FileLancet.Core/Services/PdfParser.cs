using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Xobject;

namespace FileLancet.Core.Services;

public class PdfParser : IFileLancetParser
{
    public bool CanParse(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;

        return Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public ParseResult Parse(string filePath)
    {
        try
        {
            using var document = new PdfDocument(new PdfReader(filePath));

            var details = ExtractPdfDetails(document, filePath);
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

    public async Task<ParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Parse(filePath);
        }, cancellationToken);
    }

    private PdfDetails ExtractPdfDetails(PdfDocument document, string filePath)
    {
        var docInfo = document.GetDocumentInfo();
        var fileInfo = new FileInfo(filePath);

        var authors = docInfo.GetAuthor();
        var details = new PdfDetails
        {
            Title = docInfo.GetTitle() ?? Path.GetFileNameWithoutExtension(filePath),
            Authors = string.IsNullOrEmpty(authors)
                ? new List<string>()
                : new List<string> { authors },
            Subject = docInfo.GetSubject(),
            Keywords = docInfo.GetKeywords(),
            Creator = docInfo.GetCreator(),
            Producer = docInfo.GetProducer(),
            PdfVersion = document.GetPdfVersion()?.ToString() ?? "Unknown",
            PageCount = document.GetNumberOfPages(),
            IsEncrypted = false,
            FilePath = filePath,
            FileSize = fileInfo.Length,
            LastModified = fileInfo.LastWriteTime,
            CreatedTime = fileInfo.CreationTime,
            FileExtension = ".pdf",
            MimeType = "application/pdf"
        };

        for (int i = 1; i <= document.GetNumberOfPages(); i++)
        {
            try
            {
                var page = document.GetPage(i);
                var pageDict = page.GetPdfObject();
                var rotation = pageDict.GetAsNumber(PdfName.Rotate);

                details.Pages.Add(new PdfPageInfo
                {
                    PageNumber = i,
                    Width = (float)(page.GetPageSize().GetWidth()),
                    Height = (float)(page.GetPageSize().GetHeight()),
                    Rotation = rotation != null ? rotation.IntValue() : 0
                });
            }
            catch
            {
                details.Pages.Add(new PdfPageInfo
                {
                    PageNumber = i,
                    Width = 0,
                    Height = 0,
                    Rotation = 0
                });
            }
        }

        return details;
    }

    private FileNode BuildPdfTree(PdfDocument document, string filePath)
    {
        var root = new FileNode
        {
            Name = Path.GetFileName(filePath),
            Path = filePath,
            Type = NodeType.PdfDocument,
            Description = $"PDF 文档 - {document.GetNumberOfPages()} 页, 版本 {document.GetPdfVersion()}"
        };

        AddHeaderNode(root, document, filePath);
        AddBodyNode(root, document, filePath);
        AddTrailerNode(root, document, filePath);
        AddCatalogNode(root, document, filePath);

        return root;
    }

    private void AddHeaderNode(FileNode parent, PdfDocument document, string filePath)
    {
        var headerNode = new FileNode
        {
            Name = "Header",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "PDF 文件头"
        };

        headerNode.Children.Add(new FileNode
        {
            Name = $"Version: {document.GetPdfVersion()}",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = "PDF版本号"
        });

        headerNode.Children.Add(new FileNode
        {
            Name = "Binary header marker: %PDF-",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = "文件标识"
        });

        parent.Children.Add(headerNode);
    }

    private void AddBodyNode(FileNode parent, PdfDocument document, string filePath)
    {
        var bodyNode = new FileNode
        {
            Name = "Body",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "PDF 主体内容（对象集合）"
        };

        var infoNode = new FileNode
        {
            Name = "Document Info",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "文档信息字典"
        };

        var docInfo = document.GetDocumentInfo();
        AddInfoEntry(infoNode, "Title", docInfo.GetTitle(), filePath);
        AddInfoEntry(infoNode, "Author", docInfo.GetAuthor(), filePath);
        AddInfoEntry(infoNode, "Subject", docInfo.GetSubject(), filePath);
        AddInfoEntry(infoNode, "Keywords", docInfo.GetKeywords(), filePath);
        AddInfoEntry(infoNode, "Creator", docInfo.GetCreator(), filePath);
        AddInfoEntry(infoNode, "Producer", docInfo.GetProducer(), filePath);

        bodyNode.Children.Add(infoNode);

        var pagesNode = new FileNode
        {
            Name = $"Pages ({document.GetNumberOfPages()})",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "页面树"
        };

        for (int i = 1; i <= document.GetNumberOfPages(); i++)
        {
            try
            {
                var page = document.GetPage(i);
                var pageNode = BuildPageTree(page, filePath, i);
                pagesNode.Children.Add(pageNode);
            }
            catch (Exception ex)
            {
                pagesNode.Children.Add(new FileNode
                {
                    Name = $"Page {i} (错误)",
                    Path = $"{filePath}#page={i}",
                    Type = NodeType.PdfPage,
                    Description = $"无法读取: {ex.Message}"
                });
            }
        }

        bodyNode.Children.Add(pagesNode);

        bodyNode.Children.Add(new FileNode
        {
            Name = "Outline (书签)",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "书签大纲（如果有）"
        });

        parent.Children.Add(bodyNode);
    }

    private FileNode BuildPageTree(PdfPage page, string filePath, int pageNumber)
    {
        var pageSize = page.GetPageSize();
        var pageDict = page.GetPdfObject();
        var rotation = pageDict.GetAsNumber(PdfName.Rotate);

        var pageNode = new FileNode
        {
            Name = $"Page {pageNumber}",
            Path = $"{filePath}#page={pageNumber}",
            Type = NodeType.PdfPage,
            Description = $"{pageSize.GetWidth():F0}x{pageSize.GetHeight():F0} pts, Rotation: {rotation?.IntValue() ?? 0}"
        };

        AddPageContentStream(pageNode, page, filePath);
        AddPageResources(pageNode, page, filePath);

        return pageNode;
    }

    private void AddPageContentStream(FileNode parent, PdfPage page, string filePath)
    {
        var contentNode = new FileNode
        {
            Name = "Content Stream",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "页面内容流"
        };

        var textContent = PdfTextExtractor.GetTextFromPage(page);
        var textLines = textContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var textCount = textLines.Length;

        contentNode.Children.Add(new FileNode
        {
            Name = $"Text items: {textCount}",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = $"文本项数量 ({textContent.Length} 字符)"
        });

        contentNode.Children.Add(new FileNode
        {
            Name = $"Page size: {page.GetPageSize().GetWidth():F0}x{page.GetPageSize().GetHeight():F0} pts",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = "页面尺寸"
        });

        var rotation = page.GetPdfObject().GetAsNumber(PdfName.Rotate);
        contentNode.Children.Add(new FileNode
        {
            Name = $"Rotation: {rotation?.IntValue() ?? 0}",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = "旋转角度"
        });

        var resourcesDict = page.GetResources();
        if (resourcesDict != null)
        {
            var xObjectsDict = resourcesDict.GetPdfObject().GetAsDictionary(PdfName.XObject);
            if (xObjectsDict != null && xObjectsDict.Size() > 0)
            {
                var imagesNode = new FileNode
                {
                    Name = $"Images ({xObjectsDict.Size()})",
                    Path = filePath,
                    Type = NodeType.PdfOutline,
                    Description = "页面中的图像"
                };

                int imgIndex = 1;
                foreach (var entry in xObjectsDict.EntrySet())
                {
                    var xObjName = entry.Key.ToString();
                    var xObj = entry.Value;
                    if (xObj is PdfStream stream)
                    {
                        int width = 0, height = 0;
                        string colorSpace = "Unknown";

                        var subtype = stream.GetAsName(PdfName.Subtype);
                        if (subtype != null && subtype.ToString() == "/Image")
                        {
                            var widthObj = stream.GetAsNumber(PdfName.Width);
                            var heightObj = stream.GetAsNumber(PdfName.Height);
                            var csObj = stream.GetAsName(PdfName.ColorSpace);

                            if (widthObj != null) width = widthObj.IntValue();
                            if (heightObj != null) height = heightObj.IntValue();
                            if (csObj != null) colorSpace = csObj.ToString();
                        }

                        imagesNode.Children.Add(new FileNode
                        {
                            Name = $"Image {imgIndex}: {xObjName}",
                            Path = filePath,
                            Type = NodeType.PdfImage,
                            Description = $"Type: Image XObject\nName: {xObjName}\nWidth: {width}\nHeight: {height}\nColorSpace: {colorSpace}\nPage: {filePath.Split("#page=").LastOrDefault() ?? "Unknown"}"
                        });
                    }
                    imgIndex++;
                }

                contentNode.Children.Add(imagesNode);
            }
        }

        parent.Children.Add(contentNode);
    }

    private void AddPageResources(FileNode parent, PdfPage page, string filePath)
    {
        var resourcesNode = new FileNode
        {
            Name = "Resources",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "页面资源"
        };

        var resourcesDict = page.GetResources();
        if (resourcesDict == null)
        {
            parent.Children.Add(resourcesNode);
            return;
        }

        var fontsNode = new FileNode
        {
            Name = "Fonts",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "字体资源"
        };

        var fontDict = resourcesDict.GetPdfObject().GetAsDictionary(PdfName.Font);
        if (fontDict != null && fontDict.Size() > 0)
        {
            foreach (var entry in fontDict.EntrySet())
            {
                var fontName = entry.Key.ToString();
                var fontObj = entry.Value;

                string fontType = "Unknown";
                string fontDescription = "字体信息";

                try
                {
                    if (fontObj is PdfDictionary fontRefDict)
                    {
                        var subtype = fontRefDict.GetAsName(PdfName.Subtype);
                        if (subtype != null)
                        {
                            fontType = subtype.ToString();
                        }

                        var baseFont = fontRefDict.GetAsName(PdfName.BaseFont);
                        if (baseFont != null)
                        {
                            fontDescription = $"Type: {fontType}\nBaseFont: {baseFont}\nResource: {fontName}";
                        }
                        else
                        {
                            fontDescription = $"Type: {fontType}\nResource: {fontName}";
                        }
                    }
                }
                catch
                {
                    fontDescription = $"Resource: {fontName}";
                }

                fontsNode.Children.Add(new FileNode
                {
                    Name = $"{fontName} ({fontType})",
                    Path = filePath,
                    Type = NodeType.PdfFont,
                    Description = fontDescription
                });
            }
        }
        else
        {
            fontsNode.Children.Add(new FileNode
            {
                Name = "(无字体信息)",
                Path = filePath,
                Type = NodeType.PdfFont,
                Description = "页面资源字典中无字体"
            });
        }

        resourcesNode.Children.Add(fontsNode);

        var xobjectsNode = new FileNode
        {
            Name = "XObjects",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "外部对象"
        };

        var xObjectsDict = resourcesDict.GetPdfObject().GetAsDictionary(PdfName.XObject);
        if (xObjectsDict != null && xObjectsDict.Size() > 0)
        {
            foreach (var entry in xObjectsDict.EntrySet())
            {
                var xObjName = entry.Key.ToString();
                var xObj = entry.Value;
                if (xObj is PdfStream stream)
                {
                    var subtype = stream.GetAsName(PdfName.Subtype);
                    string objType = subtype?.ToString() ?? "Unknown";

                    if (objType == "/Image")
                    {
                        int width = 0, height = 0;
                        string colorSpace = "Unknown";

                        var widthObj = stream.GetAsNumber(PdfName.Width);
                        var heightObj = stream.GetAsNumber(PdfName.Height);
                        var csObj = stream.GetAsName(PdfName.ColorSpace);

                        if (widthObj != null) width = widthObj.IntValue();
                        if (heightObj != null) height = heightObj.IntValue();
                        if (csObj != null) colorSpace = csObj.ToString();

                        xobjectsNode.Children.Add(new FileNode
                        {
                            Name = $"Image: {xObjName}",
                            Path = filePath,
                            Type = NodeType.PdfImage,
                            Description = $"Type: Image XObject\nName: {xObjName}\nWidth: {width}\nHeight: {height}\nColorSpace: {colorSpace}\nPage: {filePath.Split("#page=").LastOrDefault() ?? "Unknown"}"
                        });
                    }
                    else
                    {
                        xobjectsNode.Children.Add(new FileNode
                        {
                            Name = $"XObject: {xObjName}",
                            Path = filePath,
                            Type = NodeType.PdfImage,
                            Description = $"Type: {objType}\nPage: {filePath.Split("#page=").LastOrDefault() ?? "Unknown"}"
                        });
                    }
                }
            }
        }
        else
        {
            xobjectsNode.Children.Add(new FileNode
            {
                Name = "(无XObject)",
                Path = filePath,
                Type = NodeType.PdfImage,
                Description = "页面资源字典中无XObject"
            });
        }

        resourcesNode.Children.Add(xobjectsNode);

        var colorSpacesNode = new FileNode
        {
            Name = "ColorSpaces",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "颜色空间"
        };

        var colorSpacesDict = resourcesDict.GetPdfObject().GetAsDictionary(PdfName.ColorSpace);
        if (colorSpacesDict != null && colorSpacesDict.Size() > 0)
        {
            foreach (var entry in colorSpacesDict.EntrySet())
            {
                var csName = entry.Key.ToString();
                colorSpacesNode.Children.Add(new FileNode
                {
                    Name = csName,
                    Path = filePath,
                    Type = NodeType.PdfFont,
                    Description = $"ColorSpace: {csName}"
                });
            }
        }
        else
        {
            colorSpacesNode.Children.Add(new FileNode
            {
                Name = "(无颜色空间信息)",
                Path = filePath,
                Type = NodeType.PdfFont,
                Description = "页面资源字典中无颜色空间"
            });
        }

        resourcesNode.Children.Add(colorSpacesNode);

        parent.Children.Add(resourcesNode);
    }

    private void AddInfoEntry(FileNode parent, string label, string? value, string filePath)
    {
        parent.Children.Add(new FileNode
        {
            Name = $"{label}: {value ?? "(无)"}",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = label
        });
    }

    private void AddTrailerNode(FileNode parent, PdfDocument document, string filePath)
    {
        var trailerNode = new FileNode
        {
            Name = "Trailer",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "PDF 尾部信息"
        };

        trailerNode.Children.Add(new FileNode
        {
            Name = "Root: Catalog",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = "根对象引用"
        });

        trailerNode.Children.Add(new FileNode
        {
            Name = $"Size: {document.GetNumberOfPages()}",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = "对象总数"
        });

        trailerNode.Children.Add(new FileNode
        {
            Name = $"Encrypted: 待检查",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = "是否加密"
        });

        parent.Children.Add(trailerNode);
    }

    private void AddCatalogNode(FileNode parent, PdfDocument document, string filePath)
    {
        var catalogNode = new FileNode
        {
            Name = "Catalog",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "文档目录（根对象）"
        };

        catalogNode.Children.Add(new FileNode
        {
            Name = "Type: Catalog",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = "对象类型"
        });

        catalogNode.Children.Add(new FileNode
        {
            Name = $"Pages: {document.GetNumberOfPages()} pages",
            Path = filePath,
            Type = NodeType.PdfFont,
            Description = "页面引用"
        });

        catalogNode.Children.Add(new FileNode
        {
            Name = "Outlines",
            Path = filePath,
            Type = NodeType.PdfOutline,
            Description = "书签引用"
        });

        parent.Children.Add(catalogNode);
    }
}