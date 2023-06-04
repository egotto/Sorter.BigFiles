﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sorter.BigFiles.App
{
    internal class LargeFileSplitter2
    {
        private readonly ConfigOptions _options;

        public LargeFileSplitter2(ConfigOptions configOptions)
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

            using var fs = File.Open(
                _options.SourceFilePath,
                FileMode.Open,
                FileAccess.Read);
            using var bs = new BufferedStream(fs);
            var splitFilesCount = 0;
            if (fs.Length <= _options.MaxFileSizeInMBs * 1024 * 1024)
            {
                splitFilesCount = 1;
            }
            else
            {
                splitFilesCount = CalculateOptimalFilesCount(fs.Length);
            }

            var bufSize = (ulong)Math.Ceiling((double)fs.Length / splitFilesCount);
            var buffer = new byte[bufSize];
            var ms = new MemoryStream(buffer);
            var sr = new StreamReader(ms);
            Console.WriteLine(
                $"Approximate size of generated files will be = {splitFilesCount} * {bufSize / (1024 * 1024)}MB");
            var currentLine = new StringBuilder();
            while (bs.Read(buffer, 0, (int)bufSize) != 0)
            {
                noOfCurrentFile++;
                Console.Write($"\r Write to file {noOfCurrentFile}/{splitFilesCount}...");
                var fileName = Path.Combine(_options.OutputSplitFilesDirectory,
                    _options.OutputFilesTemplate.Replace("{i}", noOfCurrentFile.ToString()));
                if (File.Exists(fileName))
                    File.Delete(fileName);

                using var w = File.AppendText(fileName);
                ms.Seek(0, SeekOrigin.Begin);
                while (!sr.EndOfStream)
                {
                    var pattern = @"([0-9]+)\.\s([A-Za-z0-9 ]+)";
                    var replacement = @"$2|$1";
                    var currentRecord = LineFormatter(sr, currentLine);
                    if (currentRecord != null)
                    {
                        w.WriteLine(Regex.Replace(currentRecord, pattern, replacement));
                    }
                }
            }

            return noOfCurrentFile;

            static string? LineFormatter(TextReader streamReader, StringBuilder currentLineBuilder)
            {
                var charBuffer = new char[1];

                while (streamReader.Read(charBuffer, 0, 1) > 0)
                {
                    if (charBuffer[0].Equals('\n'))
                    {
                        var result = currentLineBuilder.ToString();
                        currentLineBuilder.Clear();

                        if (result.EndsWith('\r'))
                        {
                            result = result.Substring(0, result.Length - 1);
                        }

                        return result;
                    }

                    currentLineBuilder.Append(charBuffer[0]);
                }

                return null;
            }
        }

        private int CalculateOptimalFilesCount(long fileLength)
        {
            var processorsCount = Environment.ProcessorCount;
            int splitFilesCount = 0; // I use the available number of processors for future distributed sorting.
            long outputFileSize = fileLength;
            long maxFileSizeInBytes = _options.MaxFileSizeInMBs * 1024 * 1024;
            while (outputFileSize > maxFileSizeInBytes)
            {
                splitFilesCount += processorsCount;
                outputFileSize = fileLength / splitFilesCount;
            }

            return splitFilesCount;
        }
    }
}