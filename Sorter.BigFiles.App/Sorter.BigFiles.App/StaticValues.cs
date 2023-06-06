using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sorter.BigFiles.App
{
    public static class StaticValues
    {
        public static string SortedKey = "sorted";
        public static string MergedKey = "merged";
        public static long AverageLinesCountPerFile = 0;
        public static long AverageLinesCountPerThread = 0;
        public static string LineSplitSeparator = ". ";
    }
}