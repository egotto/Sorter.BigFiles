using Sorter.BigFiles.App.Models;
using System.Text;

namespace Sorter.BigFiles.App.Services;

internal class SplitFileSorter
{
    public readonly ConfigOptions _options;

    public SplitFileSorter(ConfigOptions options)
    {
        _options = options;
    }

    public void SortFiles()
    {
#if DEBUG
        if (!_options.SplitSortingEnabled)
        {
            Console.WriteLine("File sorting disabled");
            return;
        }
#endif

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

        var sortedFileName = Path.Combine(_options.OutputSplitFilesDirectory, Guid.NewGuid().ToString());

        File.Delete(fileName);

        if (File.Exists(sortedFileName))
            File.Delete(sortedFileName);

        File.WriteAllText(sortedFileName, sb.ToString());
        SemStaticPool.SemaphoreProcessing.Release();
        Console.WriteLine($"File processing ended: {fileName}");
        sb.Clear();
    }
}