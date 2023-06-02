namespace Sorter.BigFiles.Generator
{
    public class StringLineGenerator
    {
        private readonly string[] _inputStrings;
        private readonly string _outputStringFormat;
        private readonly Random _random;

        public StringLineGenerator(string outputStringFormat, string inputStringsFilePath)
        {
            _outputStringFormat = outputStringFormat;

            using var sr = new StreamReader(inputStringsFilePath);

            _inputStrings = sr.ReadToEnd()
                .Split(Environment.NewLine)
                .Where(_ => !string.IsNullOrWhiteSpace(_))
                .Select(x => x.Trim())
                .ToArray();

            _random = new Random((int)DateTime.Now.Ticks);
        }

        public string GenerateRandomString =>
            string.Format(_outputStringFormat, RandomInt, RandomStringFromInputs);

        private int RandomInt => _random.Next(1, int.MaxValue);
        private string RandomStringFromInputs => _inputStrings[_random.Next(0, _inputStrings.Length - 1)];
    }
}
