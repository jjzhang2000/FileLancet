using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileLancet.Core.Factories;
using FileLancet.Core.Interfaces;
using FileLancet.Core.Models;
using FileLancet.Core.Services;
using Microsoft.Win32;

namespace FileLancet.UI.ViewModels
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private FileNode? _selectedNode;
        private string _statusMessage = "Ready";
        private string _filePath = "";
        private bool _isLoading;
        private FileDetails? _fileDetails;
        private IContentLoader? _contentLoader;
        private readonly IPreviewService _previewService;
        private bool _isGenericFile; // 标记是否为通用文件（非EPUB等结构化文件）

        public MainViewModel()
        {
            ParserFactory.InitializeDefaults();
            FileTreeNodes = new ObservableCollection<FileNode>();
            NodeDetails = new NodeDetailsViewModel();
            Preview = new PreviewViewModel();
            _previewService = new PreviewService();

            OpenFileCommand = new RelayCommand(OpenFile);
            RefreshCommand = new RelayCommand(Refresh, () => !string.IsNullOrEmpty(FilePath));
            ExportCommand = new RelayCommand(Export, () => FileTreeNodes.Count > 0);
        }

        #region Properties

        /// <summary>
        /// 左栏数据 - 文件树节点
        /// </summary>
        public ObservableCollection<FileNode> FileTreeNodes { get; }

        /// <summary>
        /// 当前选中节点
        /// </summary>
        public FileNode? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode != value)
                {
                    _selectedNode = value;
                    OnPropertyChanged();
                    UpdateNodeDetails();
                    _ = UpdatePreviewAsync();
                }
            }
        }

        /// <summary>
        /// 中栏数据 - 节点详情
        /// </summary>
        public NodeDetailsViewModel NodeDetails { get; }

        /// <summary>
        /// 右栏数据 - 预览
        /// </summary>
        public PreviewViewModel Preview { get; }

        /// <summary>
        /// 状态栏消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 当前文件路径
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 文件详情
        /// </summary>
        public FileDetails? FileDetails
        {
            get => _fileDetails;
            set
            {
                if (_fileDetails != value)
                {
                    _fileDetails = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand OpenFileCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// 打开文件
        /// </summary>
        private void OpenFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "EPUB files (*.epub)|*.epub|All files (*.*)|*.*",
                Title = "Select EPUB File"
            };

            if (dialog.ShowDialog() == true)
            {
                _ = LoadFileAsync(dialog.FileName);
            }
        }

        /// <summary>
        /// 异步加载文件
        /// </summary>
        public async Task LoadFileAsync(string filePath)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading...";
                FilePath = filePath;

                // 释放旧的内容加载器
                if (_contentLoader is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                var parser = ParserFactory.GetParser(filePath);
                if (parser == null)
                {
                    StatusMessage = "No suitable parser found";
                    return;
                }

                // 检查是否为通用文件解析器
                _isGenericFile = parser is GenericFileParser;

                var result = await parser.ParseAsync(filePath);

                if (result.Success)
                {
                    FileTreeNodes.Clear();
                    
                    // 先创建内容加载器（在选中节点之前）
                    _contentLoader = _isGenericFile 
                        ? new GenericContentLoader(filePath)
                        : new EpubContentLoader(filePath);
                    
                    if (result.RootNode != null)
                    {
                        FileTreeNodes.Add(result.RootNode);
                        FileDetails = result.Details;
                        // 自动选中根节点（此时_contentLoader已准备好）
                        SelectedNode = result.RootNode;
                    }

                    StatusMessage = $"Loaded: {Path.GetFileName(filePath)}";
                }
                else
                {
                    StatusMessage = $"Error: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 刷新当前文件
        /// </summary>
        private void Refresh()
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                _ = LoadFileAsync(FilePath);
            }
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        private void Export()
        {
            StatusMessage = "Export not implemented yet";
        }

        /// <summary>
        /// 更新节点详情
        /// </summary>
        private void UpdateNodeDetails()
        {
            if (SelectedNode == null)
            {
                NodeDetails.Clear();
                return;
            }

            NodeDetails.NodeName = SelectedNode.Name;
            NodeDetails.NodeType = SelectedNode.Type.ToString();
            NodeDetails.NodePath = SelectedNode.Path;
            NodeDetails.FileSize = FormatFileSize(SelectedNode.Size);
            NodeDetails.MimeType = SelectedNode.MimeType;

            // 更新文件基本信息
            if (FileDetails != null)
            {
                NodeDetails.FileExtension = FileDetails.FileExtension;
                NodeDetails.LastModified = FileDetails.LastModified.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // 如果是根节点，显示 EPUB 元数据（仅对 EPUB 文件）
            if (SelectedNode.Type == NodeType.Root && FileDetails != null && 
                !string.IsNullOrEmpty(FileDetails.EpubVersion))
            {
                NodeDetails.IsEpubMetadataVisible = true;
                NodeDetails.Title = FileDetails.Title;
                NodeDetails.Authors = string.Join(", ", FileDetails.Authors);
                NodeDetails.Publisher = FileDetails.Publisher;
                NodeDetails.Language = FileDetails.Language;
                NodeDetails.Isbn = FileDetails.Isbn;
                NodeDetails.EpubVersion = FileDetails.EpubVersion;
            }
            else
            {
                NodeDetails.IsEpubMetadataVisible = false;
            }

            // 更新属性列表
            NodeDetails.UpdateProperties(SelectedNode, FileDetails);
        }

        /// <summary>
        /// 异步更新预览
        /// </summary>
        private async Task UpdatePreviewAsync()
        {
            if (SelectedNode == null)
            {
                Preview.Clear();
                return;
            }

            // 先清空之前的内容，避免前一元素预览残留
            Preview.Clear();

            if (_contentLoader == null)
            {
                // 没有内容加载器时，使用默认预览
                Preview.UpdatePreview(SelectedNode);
                return;
            }

            Preview.IsLoading = true;
            Preview.PreviewTitle = $"Preview: {SelectedNode.Name}";

            try
            {
                var result = await _previewService.GetPreviewAsync(SelectedNode, _contentLoader, _isGenericFile);
                Preview.UpdateFromPreviewResult(result);
            }
            catch (Exception ex)
            {
                // 发生异常时清空预览，避免显示前一元素内容
                Preview.Clear();
                Preview.HasError = true;
                Preview.ErrorMessage = $"Preview error: {ex.Message}";
            }
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F2} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
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
    /// 简单的 RelayCommand 实现
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
