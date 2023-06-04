using System.Text;

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
                while (!sr.EndOfStream && lines <= avrLinesInOneFile)
                {
                    sw.WriteLine(sr.ReadLine());
                    lines++;
                }
            }

            Console.WriteLine();
            return noOfCurrentFile;
        }

        private long GetAverageLinesCountPerFile(StreamReader stream, long filesCount)
        {

            var bufSize = stream.BaseStream.Length / filesCount;
            var bytesIn1000Lines = Encoding.Unicode.GetByteCount(GetFirst1000LinesFromFile(stream)) / 2;
            var avrBytesInLine = bytesIn1000Lines / 1000;
            return bufSize / avrBytesInLine;
        }

        private string GetFirst1000LinesFromFile(StreamReader stream)
        {
            stream.DiscardBufferedData();
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            int linesCount = 1000;
            var sb = new StringBuilder();
            while (!stream.EndOfStream && linesCount > 0)
            {
                sb.AppendLine(stream.ReadLine());
                linesCount--;
            }

            stream.DiscardBufferedData();
            stream.BaseStream.Seek(0, SeekOrigin.Begin);

            return sb.ToString();
        }

        private long CalculateOptimalFilesCount(long fileLength)
        {
            int splitFilesCount = 0;
            long outputFileSize = fileLength;
            long maxFileSizeInBytes = _options.MaxFileSizeInMBs * 1024 * 1024;
            while (outputFileSize > maxFileSizeInBytes)
            {
                splitFilesCount += 2;
                outputFileSize = fileLength / splitFilesCount;
            }

            return splitFilesCount;
        }
    }
}
