using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter.BigFiles.App;

public class SplitFileSorter
{
    private readonly ConfigOptions _options;
    private const string sortedKey = "sorted";
    private const string lineSplitSeparator = ". ";

    public SplitFileSorter(ConfigOptions configOptions)
    {
        _options = configOptions;
    }

    public void SortFile(object? filePath)
    {
        var fileName = filePath as string ?? string.Empty;
        SemStaticPool.SemaphoreProcessing.WaitOne();
        if(!File.Exists(fileName)) 
            throw new FileNotFoundException();
        var sortedFileName = fileName + sortedKey;

        string[] orderedLines = Array.Empty<string>();

        using (var sr = new StreamReader(fileName))
        {
            orderedLines = sr.ReadToEnd()
                .Split(Environment.NewLine)
                .Where(_ => !string.IsNullOrWhiteSpace(_))
                .Select(_=> new SortLine(_.Split(lineSplitSeparator)))
                .OrderBy(_=>_.Text)
                .ThenBy(_=>_.Number)
                .Select(_=>_.ToString())
                .AsParallel()
                .ToArray();
        }

        File.Delete(fileName);
        if(File.Exists(sortedFileName))
            File.Delete(sortedFileName);

        File.WriteAllLines(sortedFileName, orderedLines);

        SemStaticPool.SemaphoreProcessing.Release();
    }
}
