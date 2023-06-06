using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter.BigFiles.App
{
    public class LargeFileSplitter2
    {
        private readonly ConfigOptions _options;

        public LargeFileSplitter2(ConfigOptions configOptions)
        {
            _options = configOptions;
        }

        public void StartSplit()
        {
            if (!_options.FileSplitEnabled)
            {
                Console.WriteLine("Large file splitting disabled");
                return;
            }

            if (!File.Exists(_options.SourceFilePath))
                throw new FileNotFoundException(_options.SourceFilePath);

            if (Directory.Exists(_options.OutputSplitFilesDirectory))
                Directory.Delete(_options.OutputSplitFilesDirectory, true);

            Directory.CreateDirectory(_options.OutputSplitFilesDirectory);
            
            using var sr = new StreamReader(_options.SourceFilePath);
            var avrLinesCountPerThread = GetLinesCountPerThread(sr);
            StaticValues.AverageLinesCountPerThread = avrLinesCountPerThread;
            sr.Dispose();

            var sorter = new SplitFileSorter(_options);
            sorter.SortLinesAndSave(File.ReadLines(_options.SourceFilePath));
            Console.WriteLine();
        }

        private long GetLinesCountPerThread(StreamReader stream)
        {
            var srcFileSize = stream.BaseStream.Length;
            var cores = Environment.ProcessorCount;
            var availableRamInBytes = _options.AvailableRamInMBs * 1024 * 1024;
            if(srcFileSize < availableRamInBytes)
                return -1;

            var ramPerCore = availableRamInBytes / cores;
            var bytesIn1000Lines = Encoding.Unicode.GetByteCount(GetFirst1000LinesFromFile(stream)) / 2;
            var avrBytesInLine = bytesIn1000Lines / 1000;
            return (ramPerCore / avrBytesInLine) / 2;
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
    }
}