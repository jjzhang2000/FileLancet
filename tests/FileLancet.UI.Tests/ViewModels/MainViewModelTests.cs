using FileLancet.Core.Models;
using FileLancet.UI.ViewModels;

namespace FileLancet.UI.Tests.ViewModels
{
    public class MainViewModelTests
    {
        [Fact]
        public void TC_201_MainViewModel_Initialization_PropertiesSetCorrectly()
        {
            // Act
            var viewModel = new MainViewModel();

            // Assert
            Assert.NotNull(viewModel.FileTreeNodes);
            Assert.Empty(viewModel.FileTreeNodes);
            Assert.NotNull(viewModel.NodeDetails);
            Assert.NotNull(viewModel.Preview);
            Assert.Equal("Ready", viewModel.StatusMessage);
            Assert.Empty(viewModel.FilePath);
            Assert.False(viewModel.IsLoading);
            Assert.NotNull(viewModel.OpenFileCommand);
            Assert.NotNull(viewModel.RefreshCommand);
            Assert.NotNull(viewModel.ExportCommand);
        }

        [Fact]
        public void TC_202_SelectedNode_Set_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedNode))
                    propertyChangedRaised = true;
            };

            var node = new FileNode { Name = "test", Type = NodeType.Html };

            // Act
            viewModel.SelectedNode = node;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal(node, viewModel.SelectedNode);
        }

        [Fact]
        public void TC_203_LoadFileCommand_Execute_OpensFile()
        {
            // Arrange
            var viewModel = new MainViewModel();

            // Act - Command can execute (will show dialog)
            var canExecute = viewModel.OpenFileCommand.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        [Fact]
        public void TC_204_RefreshCommand_CanExecute_WhenFilePathNotEmpty()
        {
            // Arrange
            var viewModel = new MainViewModel();
            viewModel.FilePath = "";

            // Act & Assert - Cannot execute when path is empty
            Assert.False(viewModel.RefreshCommand.CanExecute(null));

            // Arrange
            viewModel.FilePath = "test.epub";

            // Act & Assert - Can execute when path is set
            Assert.True(viewModel.RefreshCommand.CanExecute(null));
        }

        [Fact]
        public void TC_205_NodeDetails_Update_WhenSelectedNodeChanges()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var node = new FileNode
            {
                Name = "chapter1.xhtml",
                Type = NodeType.Html,
                Path = "OEBPS/chapter1.xhtml",
                Size = 1024
            };

            // Act
            viewModel.SelectedNode = node;

            // Assert
            Assert.Equal("chapter1.xhtml", viewModel.NodeDetails.NodeName);
            Assert.Equal("Html", viewModel.NodeDetails.NodeType);
            Assert.Equal("OEBPS/chapter1.xhtml", viewModel.NodeDetails.NodePath);
        }

        [Fact]
        public void TC_206_Preview_Update_WhenSelectedNodeChanges()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var node = new FileNode
            {
                Name = "test.html",
                Type = NodeType.Html,
                Path = "test.html"
            };

            // Act
            viewModel.SelectedNode = node;

            // Assert
            Assert.Equal(PreviewType.Text, viewModel.Preview.PreviewType);
            Assert.Contains("test.html", viewModel.Preview.PreviewTitle);
        }

        [Fact]
        public void TC_207_IsLoading_Set_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsLoading))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.IsLoading = true;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.True(viewModel.IsLoading);
        }

        [Fact]
        public void TC_208_StatusMessage_Set_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.StatusMessage))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.StatusMessage = "Test message";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("Test message", viewModel.StatusMessage);
        }

        [Fact]
        public void TC_209_FilePath_Set_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.FilePath))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.FilePath = "test.epub";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("test.epub", viewModel.FilePath);
        }

        [Fact]
        public void TC_210_SelectedNode_Null_ClearsDetailsAndPreview()
        {
            // Arrange
            var viewModel = new MainViewModel();
            viewModel.SelectedNode = new FileNode { Name = "test", Type = NodeType.Html };

            // Act
            viewModel.SelectedNode = null;

            // Assert
            Assert.Empty(viewModel.NodeDetails.NodeName);
            Assert.Equal(PreviewType.None, viewModel.Preview.PreviewType);
        }

        [Fact]
        public void TC_211_MainViewModel_FormatFileSize_FormatsCorrectly()
        {
            // This tests the private FormatFileSize method indirectly through NodeDetails
            var viewModel = new MainViewModel();
            var node = new FileNode { Name = "test", Type = NodeType.Html, Size = 1536 };

            // Act
            viewModel.SelectedNode = node;

            // Assert - 1536 bytes should be formatted as "1.50 KB"
            Assert.Contains("KB", viewModel.NodeDetails.FileSize);
        }

        [Fact]
        public void TC_212_ExportCommand_CanExecute_WhenFileTreeNodesNotEmpty()
        {
            // Arrange
            var viewModel = new MainViewModel();

            // Act & Assert - Cannot execute when no nodes
            Assert.False(viewModel.ExportCommand.CanExecute(null));

            // Arrange - Add a node
            viewModel.FileTreeNodes.Add(new FileNode { Name = "test" });

            // Act & Assert - Can execute when nodes exist
            Assert.True(viewModel.ExportCommand.CanExecute(null));
        }

        [Fact]
        public void TC_213_MainViewModel_FileDetails_Set_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.FileDetails))
                    propertyChangedRaised = true;
            };

            var details = new FileDetails { Title = "Test Book" };

            // Act
            viewModel.FileDetails = details;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("Test Book", viewModel.FileDetails.Title);
        }

        [Fact]
        public void TC_214_MainViewModel_SelectedNode_Root_ShowsEpubMetadata()
        {
            // Arrange
            var viewModel = new MainViewModel();
            viewModel.FileDetails = new FileDetails
            {
                Title = "Test Book",
                Authors = new List<string> { "Author 1", "Author 2" },
                Publisher = "Test Publisher",
                Language = "en",
                EpubVersion = "3.0"
            };

            var rootNode = new FileNode { Name = "book.epub", Type = NodeType.Root };

            // Act
            viewModel.SelectedNode = rootNode;

            // Assert
            Assert.True(viewModel.NodeDetails.IsEpubMetadataVisible);
            Assert.Equal("Test Book", viewModel.NodeDetails.Title);
            Assert.Equal("Author 1, Author 2", viewModel.NodeDetails.Authors);
            Assert.Equal("Test Publisher", viewModel.NodeDetails.Publisher);
            Assert.Equal("en", viewModel.NodeDetails.Language);
            Assert.Equal("3.0", viewModel.NodeDetails.EpubVersion);
        }
    }
}
