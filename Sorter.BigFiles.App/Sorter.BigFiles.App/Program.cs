using Microsoft.Extensions.Configuration;
using Sorter.BigFiles.App;
using Sorter.BigFiles.App.Services;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

IConfiguration configuration = builder.Build();
var configOptions = configuration.GetSection(ConfigOptions.SectionName).Get<ConfigOptions>() ??
                    throw new ArgumentException("Bad config file");

if (configOptions.AvailableCores != 0)
    SemStaticPool.AvailableCores = configOptions.AvailableCores;

var watch = System.Diagnostics.Stopwatch.StartNew();

var splitter = new LargeFileSplitter(configOptions);
splitter.StartSplit();
var sorter = new SplitFileSorter(configOptions);
sorter.SortFiles();

var merger = new SortedFileMerger(configOptions);
var result = merger.MergeFiles();

Thread.Sleep(100);
watch.Stop();
var elapsedMs = watch.Elapsed;
Console.WriteLine($"Elapsed time is: {elapsedMs:c}");
Console.WriteLine($"Result file: {result}");

public static class SemStaticPool
{
    public static int AvailableCores = Environment.ProcessorCount;
    public static Semaphore SemaphoreProcessing = new Semaphore(AvailableCores, AvailableCores);
}