using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;
using FileLancet.Core.Utilities;

namespace FileLancet.UI.ViewModels
{
    /// <summary>
    /// 预览类型
    /// </summary>
    public enum PreviewType
    {
        None,
        Text,
        Html,
        Image,
        Binary,
        Code,
        StructuredCode
    }

    /// <summary>
    /// 预览视图模型
    /// </summary>
    public class PreviewViewModel : INotifyPropertyChanged
    {
        private PreviewType _previewType = PreviewType.None;
        private string _textContent = "";
        private ImageSource? _imageContent;
        private string _htmlContent = "";
        private bool _isBinary;
        private string _binaryInfo = "";
        private string _previewTitle = "Preview";
        private string _codeLanguage = "";
        private bool _isLoading;
        private string _errorMessage = "";
        private bool _hasError;
        private CodeStructureNode? _structuredContent;
        private bool _showStructuredView = false;

        #region Properties

        public PreviewType PreviewType
        {
            get => _previewType;
            set { _previewType = value; OnPropertyChanged(); }
        }

        public string TextContent
        {
            get => _textContent;
            set { _textContent = value; OnPropertyChanged(); }
        }

        public ImageSource? ImageContent
        {
            get => _imageContent;
            set { _imageContent = value; OnPropertyChanged(); }
        }

        public string HtmlContent
        {
            get => _htmlContent;
            set { _htmlContent = value; OnPropertyChanged(); }
        }

        public bool IsBinary
        {
            get => _isBinary;
            set { _isBinary = value; OnPropertyChanged(); }
        }

        public string BinaryInfo
        {
            get => _binaryInfo;
            set { _binaryInfo = value; OnPropertyChanged(); }
        }

        public string PreviewTitle
        {
            get => _previewTitle;
            set { _previewTitle = value; OnPropertyChanged(); }
        }

        public string CodeLanguage
        {
            get => _codeLanguage;
            set { _codeLanguage = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 结构化内容（用于树状显示）
        /// </summary>
        public CodeStructureNode? StructuredContent
        {
            get => _structuredContent;
            set { _structuredContent = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 是否显示结构化视图
        /// </summary>
        public bool ShowStructuredView
        {
            get => _showStructuredView;
            set { _showStructuredView = value; OnPropertyChanged(); }
        }

        #endregion

        #region Methods

        public void Clear()
        {
            PreviewType = PreviewType.None;
            TextContent = "";
            ImageContent = null;
            HtmlContent = "";
            IsBinary = false;
            BinaryInfo = "";
            PreviewTitle = "Preview";
            CodeLanguage = "";
            IsLoading = false;
            ErrorMessage = "";
            HasError = false;
            StructuredContent = null;
            ShowStructuredView = false;
        }

        public void UpdatePreview(FileNode node)
        {
            Clear();
            PreviewTitle = $"Preview: {node.Name}";

            switch (node.Type)
            {
                case NodeType.Html:
                case NodeType.Css:
                case NodeType.Script:
                    PreviewType = PreviewType.Text;
                    TextContent = $"[{node.Type} content would be loaded here]\n\nPath: {node.Path}";
                    break;

                case NodeType.Image:
                    PreviewType = PreviewType.Image;
                    LoadImagePlaceholder(node);
                    break;

                case NodeType.Audio:
                case NodeType.Video:
                case NodeType.Font:
                    PreviewType = PreviewType.Binary;
                    IsBinary = true;
                    BinaryInfo = $"{node.Type} file ({node.Size} bytes)";
                    break;

                case NodeType.Root:
                case NodeType.Folder:
                    PreviewType = PreviewType.None;
                    TextContent = $"Folder: {node.Name}\nContains {node.Children.Count} items";
                    break;

                default:
                    PreviewType = PreviewType.Binary;
                    IsBinary = true;
                    BinaryInfo = $"Binary file ({node.Size} bytes)";
                    break;
            }
        }

        /// <summary>
        /// 使用预览结果更新预览
        /// </summary>
        public void UpdateFromPreviewResult(PreviewResult result)
        {
            IsLoading = false;

            if (!result.Success)
            {
                // 预览失败时清空内容，避免显示前一元素内容
                Clear();
                HasError = true;
                ErrorMessage = result.ErrorMessage;
                return;
            }

            HasError = false;
            ErrorMessage = "";

            switch (result.ContentType)
            {
                case PreviewContentType.Text:
                    PreviewType = PreviewType.Text;
                    TextContent = result.TextContent;
                    CodeLanguage = "";
                    ShowStructuredView = false;
                    break;

                case PreviewContentType.Code:
                    PreviewType = PreviewType.Code;
                    TextContent = result.TextContent;
                    CodeLanguage = result.CodeLanguage;
                    // 解析结构化内容
                    ParseStructuredContent(result.TextContent, result.CodeLanguage);
                    break;

                case PreviewContentType.Html:
                    PreviewType = PreviewType.Html;
                    HtmlContent = result.TextContent;
                    // HTML 也解析为结构化内容
                    ParseStructuredContent(result.TextContent, "html");
                    break;

                case PreviewContentType.Image:
                    PreviewType = PreviewType.Image;
                    LoadImageFromBytes(result.ImageData, result.ImageFormat);
                    break;

                case PreviewContentType.Binary:
                    PreviewType = PreviewType.Binary;
                    IsBinary = true;
                    BinaryInfo = result.BinaryInfo;
                    break;

                default:
                    PreviewType = PreviewType.None;
                    break;
            }
        }

        /// <summary>
        /// 解析结构化内容
        /// </summary>
        private void ParseStructuredContent(string content, string language)
        {
            try
            {
                CodeStructureNode? structure = language.ToLowerInvariant() switch
                {
                    "xml" or "html" or "xhtml" => CodeStructureParser.ParseXml(content),
                    "css" => CodeStructureParser.ParseCss(content),
                    _ => null
                };

                if (structure != null && structure.Children.Count > 0)
                {
                    StructuredContent = structure;
                    ShowStructuredView = true;
                }
                else
                {
                    ShowStructuredView = false;
                }
            }
            catch
            {
                ShowStructuredView = false;
            }
        }

        private void LoadImagePlaceholder(FileNode node)
        {
            try
            {
                var bitmap = new WriteableBitmap(100, 100, 96, 96, System.Windows.Media.PixelFormats.Gray8, null);
                ImageContent = bitmap;
            }
            catch
            {
                PreviewType = PreviewType.Text;
                TextContent = $"[Image: {node.Name}]\nSize: {node.Size} bytes";
            }
        }

        private void LoadImageFromBytes(byte[]? imageData, string format)
        {
            if (imageData == null || imageData.Length == 0)
            {
                PreviewType = PreviewType.Binary;
                BinaryInfo = "Unable to load image data";
                return;
            }

            try
            {
                using var stream = new MemoryStream(imageData);
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                ImageContent = bitmap;
            }
            catch (Exception ex)
            {
                PreviewType = PreviewType.Binary;
                BinaryInfo = $"Error loading image: {ex.Message}";
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
