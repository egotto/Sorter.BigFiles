// See https://aka.ms/new-console-template for more information
using Sorter.BigFiles.App;
using System.Text.RegularExpressions;


//var pattern1 = @"([0-9]+)\.\s([A-Za-z0-9 ]+)";
//var pattern2 = @"([A-Za-z0-9 ]+)\|([0-9]+)";
//var replacement1 = @"$2|$1";
//var replacement2 = @"$2. $1";

//var result1 = Regex.Replace("21312312. Appleasd asaaa a", pattern1, replacement1);
//var result2 = Regex.Replace(result1, pattern2, replacement2);

var watch = System.Diagnostics.Stopwatch.StartNew();

var options = new ConfigOptions();
var splitter = new LargeFileSplitter3(options);
var filesCount = splitter.StartSplit();


Semaphore semaphore = new Semaphore(Environment.ProcessorCount, Environment.ProcessorCount);
var files = Directory.GetFiles(options.OutputSplitFilesDirectory).Where(_=>!_.Contains("sorted")).ToArray();
var sorter = new SplitFileSorter4();
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