namespace Sorter.BigFiles.Generator
{
    public interface IStringLineGenerator
    {
        public string GenerateRandomString { get; }
    }

    public class StringLineGenerator : IStringLineGenerator
    {
        private readonly string[] _inputStrings;
        private readonly string _outputStringFormat;
        private readonly Random _random;

        public StringLineGenerator(string outputStringFormat, string[] stringsForOutput)
        {
            if(string.IsNullOrEmpty(outputStringFormat))
                throw new ArgumentException(null, nameof(outputStringFormat));

            if(stringsForOutput is null || stringsForOutput.Length == 0)
                throw new ArgumentException(null, nameof(stringsForOutput));


            _outputStringFormat = outputStringFormat;
            _inputStrings = stringsForOutput;
            _random = new Random((int)DateTime.Now.Ticks);
        }

        public string GenerateRandomString =>
            string.Format(_outputStringFormat, RandomInt, RandomStringFromInputs);

        private int RandomInt => _random.Next(1, int.MaxValue);
        private string RandomStringFromInputs => _inputStrings[_random.Next(0, _inputStrings.Length - 1)];
    }
}
