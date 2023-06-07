using Sorter.BigFiles.App.Models;
using System.Text;

namespace Sorter.BigFiles.App.Services;

internal class SortedFileMerger
{
    private readonly ConfigOptions _options;

    internal SortedFileMerger(ConfigOptions configOptions)
    {
        _options = configOptions;
    }

    internal string MergeFiles()
    {
        var files = Directory.GetFiles(_options.OutputSplitFilesDirectory);

        var resultFile = MergeSplitFiles(files);

        if (resultFile.Length == 1)
            return resultFile[0];

        throw new Exception();
    }

    private string[] MergeSplitFiles(string[] files)
    {
        if (files.Length <= 1)
            return files;

        const int concFilesMerged = 3;

        var threads = new List<Thread>();
        for (int i = 0; i < files.Length; i += concFilesMerged)
        {
            var toBeMerging = files.Skip(i).Take(concFilesMerged).ToArray();
            if (toBeMerging.Length == 1)
                break;

            var t = new Thread(new ParameterizedThreadStart(MergeSelectedFilesIntoOne));
            t.Start(toBeMerging);
            threads.Add(t);
            Thread.Sleep(100);
        }

        while (threads.Any(_ => _.IsAlive))
        {
            Thread.Sleep(100);
        }

        Thread.Sleep(100);

        return MergeSplitFiles(Directory.GetFiles(_options.OutputSplitFilesDirectory));
    }

    private void MergeSelectedFilesIntoOne(object? filesArray)
    {
        var files = filesArray as string[] ?? Array.Empty<string>();
        SemaphorePool.SemaphoreProcessing.WaitOne();
        Console.WriteLine($"Started merging {files.Length} files in Thread#: {Environment.CurrentManagedThreadId}");

        var outputFileName = Path.Combine(_options.OutputSplitFilesDirectory, Guid.NewGuid().ToString());
        if (File.Exists(outputFileName))
            File.Delete(outputFileName);

        List<SortingReader> readers = new(files.Length);

        Parallel.ForEach(files, (file, state, index) => readers.Add(new SortingReader(file, (int)index)));

        var sb = new StringBuilder();
        long addedLinesCount = 0;

        while (readers.Any(_ => _.Value != null))
        {
            var values = readers
                .Where(_ => _.Value != null)
                .Select(_ => new { v = _.Value, i = _.Index })
                .ToArray();

            Array.Sort(values, (a, obj) => a.v!.CompareTo(obj.v));
            var val = values.First();

            AppendLineToFile(val.v!.SerializeToString());
            readers.First(_ => _.Index == val.i).ReadNext();
        }

        File.AppendAllText(outputFileName, sb.ToString());

        void AppendLineToFile(string line)
        {
            sb.AppendLine(line);
            if (addedLinesCount++ > StaticValues.AverageLinesCountPerFile)
            {
                File.AppendAllText(outputFileName, sb.ToString());
                sb.Clear();
                addedLinesCount = 0;
            }
        }

        SemaphorePool.SemaphoreProcessing.Release();
        Console.WriteLine($"Ended merging files in Thread#: {Environment.CurrentManagedThreadId}");
    }
}