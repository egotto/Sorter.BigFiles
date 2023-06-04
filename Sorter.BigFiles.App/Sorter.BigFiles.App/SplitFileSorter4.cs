using HPCsharp;

namespace Sorter.BigFiles.App
{
    internal class SplitFileSorter4
    {
        private const string sortedKey = "sorted";
        private const string lineSplitSeparator = ". ";

        public void SortFile(object? filePath)
        {
            var fileName = filePath as string ?? string.Empty;
            SemStaticPool.Semaphore.WaitOne();
            if (!File.Exists(fileName))
                throw new FileNotFoundException();
            var sortedFileName = fileName + sortedKey;

            string[] orderedLines = Array.Empty<string>();

            using (var sr = new StreamReader(fileName))
            {
                var lines = sr.ReadToEnd()
                    .Split(Environment.NewLine)
                    .Where(_ => !string.IsNullOrWhiteSpace(_))
                    .Select(_ => new SortLine(_.Split(lineSplitSeparator)))
                    .ToArray();
                Array.Sort(lines);
                //lines.SortMergePar();

                orderedLines = lines
                    .Select(_ => _.ToString())
                    .ToArray();
            }

            File.Delete(fileName);
            if (File.Exists(sortedFileName))
                File.Delete(sortedFileName);

            File.WriteAllLines(sortedFileName, orderedLines);

            SemStaticPool.Semaphore.Release();
        }
    }
}
