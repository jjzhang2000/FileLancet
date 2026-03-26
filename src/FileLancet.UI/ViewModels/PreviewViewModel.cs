using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FileLancet.Core.Models;

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
        Binary
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

        private void LoadImagePlaceholder(FileNode node)
        {
            // 创建一个占位符图像
            try
            {
                var bitmap = new WriteableBitmap(100, 100, 96, 96, PixelFormats.Gray8, null);
                ImageContent = bitmap;
            }
            catch
            {
                // 如果无法创建图像，显示文本信息
                PreviewType = PreviewType.Text;
                TextContent = $"[Image: {node.Name}]\nSize: {node.Size} bytes";
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
