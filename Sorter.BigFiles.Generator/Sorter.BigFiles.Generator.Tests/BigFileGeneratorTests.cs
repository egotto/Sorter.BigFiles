using FluentAssertions;
using Moq;
using Sorter.BigFiles.Generator.Tests.Helpers;

namespace Sorter.BigFiles.Generator.Tests
{
    public class BigFileGeneratorTests
    {
        private const string _outputFileName = "output.txt";
        private const long _outputSize = 10 * 1024;

        [Fact]
        public void FileGenerator_Should_CreateFile()
        {
            using var tempFolder = new TempFolder();
            var outputPath = Path.Combine(tempFolder.Folder.FullName, _outputFileName);
            var lineGeneratorMock = new Mock<IStringLineGenerator>();
            lineGeneratorMock
                .Setup(g => g.GenerateRandomString)
                .Returns(() => Guid.NewGuid().ToString());

            var generator = new BigFileGenerator(outputPath, _outputSize, lineGeneratorMock.Object);

            generator.CreateFileWithStrings(out var count);

            count.Should().BeGreaterThan(0);
            File.Exists(outputPath).Should().BeTrue();
            new FileInfo(outputPath).Length.Should().BeGreaterThanOrEqualTo(_outputSize);
        }
    }
}
