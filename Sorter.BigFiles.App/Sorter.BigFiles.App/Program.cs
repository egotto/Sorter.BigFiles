// See https://aka.ms/new-console-template for more information
using Sorter.BigFiles.App;

var watch = System.Diagnostics.Stopwatch.StartNew();

var options = new ConfigOptions();
var splitter = new LargeFileSplitter(options);
splitter.StartSplit();


Semaphore semaphore = new Semaphore(Environment.ProcessorCount, Environment.ProcessorCount);
var files = Directory.GetFiles(options.OutputSplitFilesDirectory).Where(_=>!_.Contains("sorted")).ToArray();
var sorter = new SplitFileSorter(options);
foreach (var file in files)
{
    var t = new Thread(new ParameterizedThreadStart(sorter.SortFile));
    t.Start(file);
}

while (Directory.GetFiles(options.OutputSplitFilesDirectory).Any(_ => !_.Contains("sorted")))
{
    Thread.Sleep(100);
}

Thread.Sleep(500);

watch.Stop();
var elapsedMs = watch.Elapsed;
Console.WriteLine($"Elapsed time is: {elapsedMs:c}");

public static class SemStaticPool
{
    public static Semaphore Semaphore = new Semaphore(Environment.ProcessorCount, Environment.ProcessorCount);
}