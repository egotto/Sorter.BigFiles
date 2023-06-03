using FluentAssertions;

namespace Sorter.BigFiles.Generator.Tests
{
    public class StringLineGeneratorTests
    {
        private const string _outputFormat = "{0}. {1}";
        private readonly string[] _inputStrings = new[]
        {
            "Apple",
            "Frog",
            "Yellow pineapple",
            "Grape"
        };

        [Fact]
        public void LineGenerator_Should_ReturnString()
        {
            var generator = new StringLineGenerator(_outputFormat, _inputStrings);

            var result = generator.GenerateRandomString;

            result.Should().BeOfType<string>();
            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void LineGenerator_Should_ReturnRandomStrings()
        {
            var generator = new StringLineGenerator(_outputFormat, _inputStrings);

            var result1 = generator.GenerateRandomString;
            var result2 = generator.GenerateRandomString;

            result1.Should().NotBeEquivalentTo(result2);
        }

        [Fact]
        public void StringLineGenerator_Should_ThrowException_If_InputStringsEmpty() 
        {
            IStringLineGenerator generator;
            var act = () => generator = new StringLineGenerator(_outputFormat, Enumerable.Empty<string>().ToArray());

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void StringLineGenerator_Should_ThrowException_If_OutputFormatEmpty()
        {
            IStringLineGenerator generator;
            var act = () => generator = new StringLineGenerator(string.Empty, _inputStrings);

            act.Should().Throw<ArgumentException>();
        }
    }
}