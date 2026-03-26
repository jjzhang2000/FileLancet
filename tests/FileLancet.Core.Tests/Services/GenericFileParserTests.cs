using FileLancet.Core.Services;
using Xunit;

namespace FileLancet.Core.Tests.Services
{
    /// <summary>
    /// GenericFileParser 单元测试 (TC-701 ~ TC-708)
    /// </summary>
    public class GenericFileParserTests
    {
        private readonly GenericFileParser _parser;

        public GenericFileParserTests()
        {
            _parser = new GenericFileParser();
        }

        [Fact]
        public void TC_701_CanParse_ExistingFile_ReturnsTrue()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                var result = _parser.CanParse(tempFile);

                // Assert
                Assert.True(result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void TC_702_CanParse_NonExistingFile_ReturnsFalse()
        {
            // Arrange
            var nonExistingFile = @"C:\NonExisting\File.txt";

            // Act
            var result = _parser.CanParse(nonExistingFile);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TC_703_CanParse_NullPath_ReturnsFalse()
        {
            // Act
            var result = _parser.CanParse(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TC_704_CanParse_EmptyPath_ReturnsFalse()
        {
            // Act
            var result = _parser.CanParse("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TC_705_Parse_ExistingFile_ReturnsSuccess()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var testContent = "Test content for generic parser";
            File.WriteAllText(tempFile, testContent);

            try
            {
                // Act
                var result = _parser.Parse(tempFile);

                // Assert
                Assert.True(result.Success);
                Assert.NotNull(result.RootNode);
                Assert.Equal(Path.GetFileName(tempFile), result.RootNode.Name);
                Assert.NotNull(result.Details);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void TC_706_Parse_NonExistingFile_ReturnsFailure()
        {
            // Arrange
            var nonExistingFile = @"C:\NonExisting\File.txt";

            // Act
            var result = _parser.Parse(nonExistingFile);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("不存在", result.ErrorMessage);
        }

        [Fact]
        public void TC_707_Parse_ReturnsCorrectFileDetails()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var testContent = "Test content";
            File.WriteAllText(tempFile, testContent);
            var fileInfo = new FileInfo(tempFile);

            try
            {
                // Act
                var result = _parser.Parse(tempFile);

                // Assert
                Assert.True(result.Success);
                Assert.NotNull(result.Details);
                Assert.Equal(fileInfo.Name, result.Details.Title);
                Assert.Equal(fileInfo.FullName, result.Details.FilePath);
                Assert.Equal(fileInfo.Length, result.Details.FileSize);
                Assert.Equal(fileInfo.Extension, result.Details.FileExtension);
                Assert.NotEmpty(result.Details.MimeType);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void TC_708_Parse_RootNodeHasNoChildren()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "test");

            try
            {
                // Act
                var result = _parser.Parse(tempFile);

                // Assert
                Assert.True(result.Success);
                Assert.NotNull(result.RootNode);
                Assert.Empty(result.RootNode.Children);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
