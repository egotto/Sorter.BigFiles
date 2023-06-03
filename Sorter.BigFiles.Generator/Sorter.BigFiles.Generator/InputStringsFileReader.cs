namespace Sorter.BigFiles.Generator
{

    public interface IInputStringsFileReader
    {
        string[] ReadInputStrings(string filePath);
    }
    public class InputStringsFileReader : IInputStringsFileReader
    {
        public string[] ReadInputStrings(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            using var sr = new StreamReader(filePath);

            return sr.ReadToEnd()
                .Split(Environment.NewLine)
                .Where(_ => !string.IsNullOrWhiteSpace(_))
                .Select(x => x.Trim())
                .ToArray();
        }
    }
}
