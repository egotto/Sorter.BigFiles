using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter.BigFiles.App;

public class BigFileSorter
{
    private readonly ConfigOptions _options;

    public BigFileSorter(ConfigOptions configOptions)
    {
        _options = configOptions;
    }

    public void SortBigFileIntoSmallers()
    {
        using var fs = File.Open(
            _options.SourceFilePath,
            FileMode.Open,
            FileAccess.Read);
    }
}
