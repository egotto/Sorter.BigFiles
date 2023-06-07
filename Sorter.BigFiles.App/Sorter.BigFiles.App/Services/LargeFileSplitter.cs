using System.Text;

namespace Sorter.BigFiles.App.Services;

internal class LargeFileSplitter
{
    private readonly ConfigOptions _options;

    public LargeFileSplitter(ConfigOptions configOptions)
    {
        _options = configOptions;
    }

    public uint StartSplit()
    {
#if DEBUG
        if (!_options.FileSplitEnabled)
        {
            Console.WriteLine("Large file splitting disabled");
            return 0;
        }
#endif

        if (!File.Exists(_options.SourceFilePath))
            throw new FileNotFoundException(_options.SourceFilePath);

        Console.WriteLine("Start splitting dataset file");

        uint noOfCurrentFile = 0;

        if (Directory.Exists(_options.OutputSplitFilesDirectory))
            Directory.Delete(_options.OutputSplitFilesDirectory, true);

        Directory.CreateDirectory(_options.OutputSplitFilesDirectory);

        using var sr = new StreamReader(_options.SourceFilePath);
        var splitFilesCount = CalculateOptimalFilesCount(sr.BaseStream.Length);

        if (splitFilesCount == -1)
        {
            Console.WriteLine("The file is placed in the allocated RAM and will be processed in its entirety");
            sr.Dispose();
            var outputFilePath = Path.Combine(_options.OutputSplitFilesDirectory,
                _options.OutputFilesTemplate.Replace("{i}", 1.ToString()));
            File.Copy(_options.SourceFilePath, outputFilePath);

            return 1;
        }

        var avrLinesInOneFile = GetAverageLinesCountPerFile(sr, splitFilesCount);
        StaticValues.AverageLinesCountPerFile = avrLinesInOneFile;

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
        var cores = Environment.ProcessorCount;
        int splitFilesCount = 0;
        long outputFileSize = fileLength;
        // we use half of the available memory in the calculation,
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

}