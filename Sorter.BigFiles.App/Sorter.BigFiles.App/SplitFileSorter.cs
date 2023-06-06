using System.Text;
using HPCsharp;

namespace Sorter.BigFiles.App
{
    internal class SplitFileSorter
    {
        public readonly ConfigOptions _options;

        public SplitFileSorter(ConfigOptions options)
        {
            _options = options;
        }

        public void SortFiles()
        {
            if (!_options.SplitSortingEnabled)
            {
                Console.WriteLine("File sorting disabled");
                return;
            }

            var files = Directory.GetFiles(_options.OutputSplitFilesDirectory).Where(_ => !_.Contains("sorted")).ToArray();
            var threads = new List<Thread>();
            foreach (var file in files)
            {
                var t = new Thread(new ParameterizedThreadStart(SortFile));
                t.Start(file);
                threads.Add(t);
                Thread.Sleep(100);
            }

            while (threads.Any(_ => _.IsAlive))
            {
                Thread.Sleep(100);
            }
        }

        public void SortLinesAndSave(IEnumerable<string> unsortedFileStream)
        {
            var threads = new List<Thread>();
            long readedlines = 0;
            if (StaticValues.AverageLinesCountPerThread == -1)
            {
                var t = new Thread(new ParameterizedThreadStart(SortLinesAndSaveToFile));
                t.Start(unsortedFileStream);
                threads.Add(t);
                Thread.Sleep(100);
            }
            else
            {
                string[] buffer = new string[StaticValues.AverageLinesCountPerThread];
                foreach (var line in unsortedFileStream)
                {
                    buffer[readedlines++] = line;
                    if (readedlines < StaticValues.AverageLinesCountPerThread)
                        continue;

                    var t = new Thread(new ParameterizedThreadStart(SortLinesAndSaveToFile));
                    t.Start(buffer);
                    threads.Add(t);
                    Thread.Sleep(100);
                    buffer = new string[StaticValues.AverageLinesCountPerThread];
                    readedlines = 0;
                }

                if (buffer.Any())
                {
                    var t = new Thread(new ParameterizedThreadStart(SortLinesAndSaveToFile));
                    t.Start(buffer);
                    threads.Add(t);
                    Thread.Sleep(100);
                    buffer = new string[StaticValues.AverageLinesCountPerThread];
                }
            }

            while (threads.Any(_ => _.IsAlive))
            {
                Thread.Sleep(100);
            }
        }

        private void SortLinesAndSaveToFile(object? inputEnumerableLines)
        {
            SemStaticPool.SemaphoreProcessing.WaitOne();
            var lines = inputEnumerableLines as IEnumerable<string> ?? Enumerable.Empty<string>();

            SaveSortedObjectsToFile(StringsToSortedObjects(lines));
            SemStaticPool.SemaphoreProcessing.Release();
        }

        private SortLine[] StringsToSortedObjects(IEnumerable<string> unsortedStrings)
        {
            Console.WriteLine("Lines sorting started");
            var lines = unsortedStrings
            .Where(_ => !string.IsNullOrEmpty(_))
            .Select(_ => new SortLine(_))
            .ToArray();

            // Array.Sort(lines);
            lines.SortMergeInPlacePar();
            
            Console.WriteLine("Lines sorting ended");
            return lines;
        }

        private void SaveSortedObjectsToFile(SortLine[] lines)
        {
            var sortedFileName = Path.Combine(_options.OutputSplitFilesDirectory, Guid.NewGuid().ToString());
            Console.WriteLine($"File saving started: {sortedFileName}");

            var savingEnumerable = lines.Select(_ => _.ToString());

            if (File.Exists(sortedFileName))
                File.Delete(sortedFileName);

            File.WriteAllLines(sortedFileName, savingEnumerable);
            lines = Array.Empty<SortLine>();
            Console.WriteLine($"File saving ended: {sortedFileName}");
        }

        private void SortFile(object? filePath)
        {
            var fileName = filePath as string ?? string.Empty;
            SemStaticPool.SemaphoreProcessing.WaitOne();
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            Console.WriteLine($"File processing started: {fileName}");

            var lines = File.ReadLines(fileName)
                .Where(_ => !string.IsNullOrEmpty(_))
                .Select(_ => new SortLine(_))
                .ToArray();

            Array.Sort(lines);

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                sb.AppendLine(lines[i].ToString());
            }

            lines = Array.Empty<SortLine>();

            var sortedFileName = fileName + StaticValues.SortedKey;

            File.Delete(fileName);

            if (File.Exists(sortedFileName))
                File.Delete(sortedFileName);

            File.WriteAllText(sortedFileName, sb.ToString());
            SemStaticPool.SemaphoreProcessing.Release();
            Console.WriteLine($"File processing ended: {fileName}");
            sb.Clear();
        }
    }
}
