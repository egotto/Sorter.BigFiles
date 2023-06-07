using System.Text;

namespace Sorter.BigFiles.App.Services;

internal class LargeFileSplitter
{
    private readonly ConfigOptions _options;

    internal LargeFileSplitter(ConfigOptions configOptions)
    {
        _options = configOptions;
    }

    internal int StartSplit()
    {
#if DEBUG
        if (!_options.FileSplitEnabled)
        {
            Console.WriteLine("Large file splitting disabled");
            return 0;
        }
#endif

        ValidateSourceFileAndCreateOutputDirectory();

        Console.WriteLine("Start splitting dataset file");

        var noOfCurrentFile = 0;
        using var sr = new StreamReader(_options.SourceFilePath);
        // Calculate approximate count of split files
        var splitFilesCount = CalculateOptimalFilesCount(sr.BaseStream.Length);
        // Calculate average count of lines per file for a better dividing source file
        var avrLinesInOneFile = GetAverageLinesCountPerFile(sr, splitFilesCount);
        StaticValues.AverageLinesCountPerFile = avrLinesInOneFile;

        while (!sr.EndOfStream)
        {
            noOfCurrentFile++;
            Console.Write($"\r Write to file {noOfCurrentFile}/{splitFilesCount}...");
            var tempSplitFilePath = GenerateTempSplitFilePath(noOfCurrentFile);

            var lines = 0L;
            using var sw = File.AppendText(tempSplitFilePath);
            while (!sr.EndOfStream && lines <= avrLinesInOneFile)
            {
                sw.WriteLine(sr.ReadLine());
                lines++;
            }
        }

        Console.WriteLine();
        return noOfCurrentFile;
    }

    private string GenerateTempSplitFilePath(int noOfFile)
    {
        var tempSplitFilePath = Path.Combine(_options.OutputSplitFilesDirectory,
                _options.OutputFilesTemplate.Replace("{i}", noOfFile.ToString()));
        if (File.Exists(tempSplitFilePath))
            File.Delete(tempSplitFilePath);

        return tempSplitFilePath;
    }

    private void ValidateSourceFileAndCreateOutputDirectory()
    {
        if (!File.Exists(_options.SourceFilePath))
            throw new FileNotFoundException(_options.SourceFilePath);

        if (Directory.Exists(_options.OutputSplitFilesDirectory))
            Directory.Delete(_options.OutputSplitFilesDirectory, true);

        Directory.CreateDirectory(_options.OutputSplitFilesDirectory);
    }

    private long CalculateOptimalFilesCount(long fileLength)
    {
        var cores = Environment.ProcessorCount;
        int splitFilesCount = 0;
        long outputFileSize = fileLength;
        // I use half of the available memory in the calculation,
        // because the file when sorted will be doubled in size in memory.
        var availableRamInBytes = (_options.AvailableRamInMBs * 1024 * 1024) / 2;

        var maxFileSizeInBytes = availableRamInBytes / cores;
        while (outputFileSize > maxFileSizeInBytes)
        {
            splitFilesCount += 2;
            outputFileSize = fileLength / splitFilesCount;
        }

        return splitFilesCount;
    }

    private static long GetAverageLinesCountPerFile(StreamReader stream, long filesCount)
    {
        var bufSize = stream.BaseStream.Length / filesCount;
        var bytesIn1000Lines = Encoding.Unicode.GetByteCount(GetFirst1000LinesFromFile(stream)) / 2;
        var avrBytesInLine = bytesIn1000Lines / 1000;
        return bufSize / avrBytesInLine;
    }

    private static string GetFirst1000LinesFromFile(StreamReader stream)
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