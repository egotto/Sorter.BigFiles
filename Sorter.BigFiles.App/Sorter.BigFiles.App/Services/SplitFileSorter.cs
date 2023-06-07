using Sorter.BigFiles.App.Models;

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

        var files = Directory.GetFiles(_options.OutputSplitFilesDirectory);
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
        SemaphorePool.SemaphoreProcessing.WaitOne();
        if (!File.Exists(fileName))
            throw new FileNotFoundException();

        Console.WriteLine($"File processing started: {fileName}. Thread#: {Environment.CurrentManagedThreadId}");

        // Deserialization of input file to sorting objects
        var lines = File.ReadLines(fileName)
            .Where(_ => !string.IsNullOrEmpty(_))
            .Select(_ => new SortLine(_))
            .ToArray();

        Array.Sort(lines);

        var savingEnumerable = lines.Select(_ => _.SerializeToString());
        var sortedFileName = Path.Combine(_options.OutputSplitFilesDirectory, Guid.NewGuid().ToString());

        File.Delete(fileName);
        if (File.Exists(sortedFileName))
            File.Delete(sortedFileName);

        File.WriteAllLines(sortedFileName, savingEnumerable);
        SemaphorePool.SemaphoreProcessing.Release();
        Console.WriteLine($"File processing ended: {fileName}. Thread#: {Environment.CurrentManagedThreadId}");
    }
}