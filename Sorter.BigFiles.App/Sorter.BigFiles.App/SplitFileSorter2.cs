using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sorter.BigFiles.App
{
    internal class SplitFileSorter2
    {
        private const string sortedKey = "sorted";

        public SplitFileSorter2()
        {
        }

        public void SortFile(object? filePath)
        {
            var fileName = filePath as string ?? string.Empty;
            SemStaticPool.Semaphore.WaitOne();
            if (!File.Exists(fileName))
                throw new FileNotFoundException();
            var sortedFileName = fileName + sortedKey;

            string[] orderedLines = Array.Empty<string>();
            var pattern1 = @"([0-9]+)\.\s([A-Za-z0-9 ]+)";
            var pattern2 = @"([A-Za-z0-9 ]+)\|([0-9]+)";
            var replacement1 = @"$2|$1";
            var replacement2 = @"$2. $1";
            using (var sr = new StreamReader(fileName))
            {
                orderedLines = sr.ReadToEnd()
                    .Split(Environment.NewLine)
                    .Select(_ => Regex.Replace(_, pattern1, replacement1))
                    .ToArray();
                Array.Sort(orderedLines);
                orderedLines = orderedLines
                    .Select(_ => Regex.Replace(_, pattern2, replacement2))
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
