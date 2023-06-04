using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Sorter.BigFiles.App
{
    internal class LargeFileSplitter3
    {
        private readonly ConfigOptions _options;

        public LargeFileSplitter3(ConfigOptions configOptions)
        {
            _options = configOptions;
        }

        public uint StartSplit()
        {
            if (!_options.FileSplitEnabled)
            {
                Console.WriteLine("Large file splitting disabled");
                return 0;
            }

            if (!File.Exists(_options.SourceFilePath))
                throw new FileNotFoundException(_options.SourceFilePath);

            Console.WriteLine("Start splitting dataset file");

            uint noOfCurrentFile = 0;

            if (!Directory.Exists(_options.OutputSplitFilesDirectory))
                Directory.CreateDirectory(_options.OutputSplitFilesDirectory);

            using var sr = new StreamReader(_options.SourceFilePath);
            var splitFilesCount = CalculateOptimalFilesCount(sr.BaseStream.Length);
            var avrLinesInOneFile = GetAverageLinesCountPerFile(sr, splitFilesCount);

            while (!sr.EndOfStream)
            {
                noOfCurrentFile++;
                Console.Write($"\r Write to file {noOfCurrentFile}/{splitFilesCount}...");
                var fileName = Path.Combine(_options.OutputSplitFilesDirectory,
                    _options.OutputFilesTemplate.Replace("{i}", noOfCurrentFile.ToString()));
                if (File.Exists(fileName))
                    File.Delete(fileName);

                var lines = 0L;
                using var sw = File.AppendText(fileName);
                while(!sr.EndOfStream && lines <= avrLinesInOneFile)
                {
                    sw.WriteLine(sr.ReadLine());
                    lines++;
                    //var pattern = @"([0-9]+)\.\s([A-Za-z0-9 ]+)";
                    //var replacement = @"$2|$1";
                    //sw.WriteLine(Regex.Replace(sr.ReadLine(), pattern, replacement));
                    //lines++;
                }
            }

            Console.WriteLine();
            return noOfCurrentFile;
        }

        private long GetAverageLinesCountPerFile(StreamReader stream, int filesCount)
        {
            
            var bufSize = (long)Math.Ceiling((double)stream.BaseStream.Length / filesCount);
            var bytesIn100Lines = Encoding.Unicode.GetByteCount(GetFirst1000LinesFromFile(stream));
            var avrBytesInLine = bytesIn100Lines / 1000;
            return bufSize / avrBytesInLine;
        }

        private string GetFirst1000LinesFromFile(StreamReader stream)
        {
            stream.DiscardBufferedData();
            int linesCount = 1000;
            var sb = new StringBuilder();
            while (!stream.EndOfStream && linesCount > 0)
            {
                sb.AppendLine(stream.ReadLine());
                linesCount--;
            }
            stream.DiscardBufferedData();

            return sb.ToString();
        }

        private int CalculateOptimalFilesCount(long fileLength)
        {
            var processorsCount = Environment.ProcessorCount;
            int splitFilesCount = 0; // I use the available number of processors for future distributed sorting.
            long outputFileSize = fileLength;
            long maxFileSizeInBytes = _options.MaxFileSizeInMBs * 1024 * 1024;
            while (outputFileSize > maxFileSizeInBytes)
            {
                splitFilesCount += processorsCount / 2;
                outputFileSize = fileLength / splitFilesCount;
            }

            return splitFilesCount;
        }
    }
}
