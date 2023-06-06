using Sorter.BigFiles.App;

var watch = System.Diagnostics.Stopwatch.StartNew();
var options = new ConfigOptions();

var splitter = new LargeFileSplitter(options);
var filesCount = splitter.StartSplit();

var sorter = new SplitFileSorter(options);
sorter.SortFiles();
var merger = new SortedFileMerger2(options);
var result = merger.MergeFiles();

Thread.Sleep(100);
watch.Stop();
var elapsedMs = watch.Elapsed;
Console.WriteLine($"Elapsed time is: {elapsedMs:c}");
Console.WriteLine($"Result file: {result}");

public static class SemStaticPool
{
    public static Semaphore SemaphoreProcessing = new Semaphore((int)Math.Ceiling(Environment.ProcessorCount * 0.75), (int)Math.Ceiling(Environment.ProcessorCount * 0.75));
}