// See https://aka.ms/new-console-template for more information
using Sorter.BigFiles.App;

var watch = System.Diagnostics.Stopwatch.StartNew();

var options = new ConfigOptions();
var splitter = new LargeFileSplitter3(options);
var filesCount = splitter.StartSplit();


var files = Directory.GetFiles(options.OutputSplitFilesDirectory).Where(_ => !_.Contains("sorted")).ToArray();
var sorter = new SplitFileSorter4(options);
foreach (var file in files)
{
    var t = new Thread(new ParameterizedThreadStart(sorter.SortFile));
    t.Start(file);
    Thread.Sleep(100);
}

while (Directory.GetFiles(options.OutputSplitFilesDirectory).Any(_ => !_.Contains("sorted")))
{
    Thread.Sleep(100);
}

Thread.Sleep(500);

var merger = new SortedFileMerger2(options);
var result = merger.MergeFiles();

watch.Stop();
var elapsedMs = watch.Elapsed;
Console.WriteLine($"Elapsed time is: {elapsedMs:c}");

public static class SemStaticPool
{
    public static Semaphore SemaphoreProcessing = new Semaphore(Environment.ProcessorCount / 2, Environment.ProcessorCount / 2);
    public static Semaphore SemaphoreFileReader = new Semaphore(4, 4);
}