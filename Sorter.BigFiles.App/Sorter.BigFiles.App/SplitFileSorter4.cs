using HPCsharp;

namespace Sorter.BigFiles.App
{
    internal class SplitFileSorter4
    {
        public readonly ConfigOptions _options;

        public SplitFileSorter4(ConfigOptions options)
        {
            _options = options;
        }

        public void SortFile(object? filePath)
        {
            if (!_options.SplitSortingEnabled)
            {
                Console.WriteLine("File sorting disabled");
                return;
            }

            var fileName = filePath as string ?? string.Empty;
            SemStaticPool.SemaphoreProcessing.WaitOne();
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            var sortedFileName = fileName + StaticValues.SortedKey;

            string[] orderedLines = Array.Empty<string>();

            using (var sr = new StreamReader(fileName))
            {
                Console.WriteLine($"File processing started: {fileName}");
                SemStaticPool.SemaphoreFileReader.WaitOne();
                var lines = sr.ReadToEnd()
                    .Split(Environment.NewLine)
                    .Where(_ => !string.IsNullOrWhiteSpace(_))
                    .Select(_ => new SortLine(_.Split(StaticValues.LineSplitSeparator)))
                    .ToArray();
                SemStaticPool.SemaphoreFileReader.Release();

                Array.Sort(lines);
                // lines.SortMergePar();
                orderedLines = lines
                    .Select(_ => _.ToString())
                    .ToArray();

                Console.WriteLine($"File processing ended: {fileName}");
            }

            File.Delete(fileName);
            if (File.Exists(sortedFileName))
                File.Delete(sortedFileName);

            SemStaticPool.SemaphoreFileReader.WaitOne();
            File.WriteAllLines(sortedFileName, orderedLines);
            SemStaticPool.SemaphoreFileReader.Release();

            SemStaticPool.SemaphoreProcessing.Release();
        }
    }
}
