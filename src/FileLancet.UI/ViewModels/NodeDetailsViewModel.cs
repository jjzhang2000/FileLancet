using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileLancet.Core.Models;

namespace FileLancet.UI.ViewModels
{
    /// <summary>
    /// 节点详情视图模型
    /// </summary>
    public class NodeDetailsViewModel : INotifyPropertyChanged
    {
        private string _nodeName = "";
        private string _nodeType = "";
        private string _nodePath = "";
        private string _fileSize = "";
        private string _fileExtension = "";
        private string _mimeType = "";
        private string _lastModified = "";
        private bool _isEpubMetadataVisible;
        private string _title = "";
        private string _authors = "";
        private string _publisher = "";
        private string _language = "";
        private string _isbn = "";
        private string _epubVersion = "";

        public NodeDetailsViewModel()
        {
            Properties = new ObservableCollection<PropertyItem>();
        }

        #region Properties

        public string NodeName
        {
            get => _nodeName;
            set { _nodeName = value; OnPropertyChanged(); }
        }

        public string NodeType
        {
            get => _nodeType;
            set { _nodeType = value; OnPropertyChanged(); }
        }

        public string NodePath
        {
            get => _nodePath;
            set { _nodePath = value; OnPropertyChanged(); }
        }

        public string FileSize
        {
            get => _fileSize;
            set { _fileSize = value; OnPropertyChanged(); }
        }

        public string FileExtension
        {
            get => _fileExtension;
            set { _fileExtension = value; OnPropertyChanged(); }
        }

        public string MimeType
        {
            get => _mimeType;
            set { _mimeType = value; OnPropertyChanged(); }
        }

        public string LastModified
        {
            get => _lastModified;
            set { _lastModified = value; OnPropertyChanged(); }
        }

        public bool IsEpubMetadataVisible
        {
            get => _isEpubMetadataVisible;
            set { _isEpubMetadataVisible = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Authors
        {
            get => _authors;
            set { _authors = value; OnPropertyChanged(); }
        }

        public string Publisher
        {
            get => _publisher;
            set { _publisher = value; OnPropertyChanged(); }
        }

        public string Language
        {
            get => _language;
            set { _language = value; OnPropertyChanged(); }
        }

        public string Isbn
        {
            get => _isbn;
            set { _isbn = value; OnPropertyChanged(); }
        }

        public string EpubVersion
        {
            get => _epubVersion;
            set { _epubVersion = value; OnPropertyChanged(); }
        }

        public ObservableCollection<PropertyItem> Properties { get; }

        #endregion

        #region Methods

        public void Clear()
        {
            NodeName = "";
            NodeType = "";
            NodePath = "";
            FileSize = "";
            FileExtension = "";
            MimeType = "";
            LastModified = "";
            IsEpubMetadataVisible = false;
            Title = "";
            Authors = "";
            Publisher = "";
            Language = "";
            Isbn = "";
            EpubVersion = "";
            Properties.Clear();
        }

        public void UpdateProperties(FileNode node, FileDetails? fileDetails)
        {
            Properties.Clear();

            // 物理属性
            Properties.Add(new PropertyItem { Name = "Name", Value = node.Name, Category = "Physical" });
            Properties.Add(new PropertyItem { Name = "Type", Value = node.Type.ToString(), Category = "Physical" });
            Properties.Add(new PropertyItem { Name = "Path", Value = node.Path, Category = "Physical" });
            Properties.Add(new PropertyItem { Name = "Size", Value = FileSize, Category = "Physical" });

            // EPUB 元数据（仅根节点）
            if (node.Type == Core.Models.NodeType.Root && fileDetails != null)
            {
                if (!string.IsNullOrEmpty(fileDetails.Title))
                    Properties.Add(new PropertyItem { Name = "Title", Value = fileDetails.Title, Category = "Metadata" });
                if (fileDetails.Authors.Count > 0)
                    Properties.Add(new PropertyItem { Name = "Authors", Value = string.Join(", ", fileDetails.Authors), Category = "Metadata" });
                if (!string.IsNullOrEmpty(fileDetails.Publisher))
                    Properties.Add(new PropertyItem { Name = "Publisher", Value = fileDetails.Publisher, Category = "Metadata" });
                if (!string.IsNullOrEmpty(fileDetails.Language))
                    Properties.Add(new PropertyItem { Name = "Language", Value = fileDetails.Language, Category = "Metadata" });
                if (!string.IsNullOrEmpty(fileDetails.EpubVersion))
                    Properties.Add(new PropertyItem { Name = "Version", Value = fileDetails.EpubVersion, Category = "Metadata" });
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

    /// <summary>
    /// 属性项
    /// </summary>
    public class PropertyItem : INotifyPropertyChanged
    {
        private string _name = "";
        private string _value = "";
        private string _category = "";

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
