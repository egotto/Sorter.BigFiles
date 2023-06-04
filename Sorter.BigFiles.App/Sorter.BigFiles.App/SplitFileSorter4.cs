﻿using HPCsharp;

namespace Sorter.BigFiles.App
{
    internal class SplitFileSorter4
    {
        private const string sortedKey = "sorted";
        private const string lineSplitSeparator = ". ";

        public void SortFile(object? filePath)
        {
            var fileName = filePath as string ?? string.Empty;
            SemStaticPool.SemaphoreProcessing.WaitOne();
            if (!File.Exists(fileName))
                throw new FileNotFoundException();
            var sortedFileName = fileName + sortedKey;

            string[] orderedLines = Array.Empty<string>();

            using (var sr = new StreamReader(fileName))
            {
                Console.WriteLine($"File processing started: {fileName}");
                SemStaticPool.SemaphoreFileReader.WaitOne();
                var lines = sr.ReadToEnd()
                    .Split(Environment.NewLine)
                    .Where(_ => !string.IsNullOrWhiteSpace(_))
                    .Select(_ => new SortLine(_.Split(lineSplitSeparator)))
                    .ToArray();
                SemStaticPool.SemaphoreFileReader.Release();

                lines.SortMergePar();
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
