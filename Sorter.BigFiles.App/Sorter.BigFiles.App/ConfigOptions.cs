﻿namespace Sorter.BigFiles.App
{
    public class ConfigOptions
    {
        public bool FileSplitEnabled { get; set; } = true;
        public bool SplitSortingEnabled { get; set; } = true;
        public long MaxFileSizeInMBs { get; set; } = 50;
        public long AvailableRamInMBs { get; set; } = 5000;
        public string SplitFilesSubDirectory { get; set; } = "split";
        public string OutputFilesTemplate { get; set; } = "document_{i}.txt";
        public string SourceFileName { get; set; } = "file2sort10.txt";
        public string SourceFileDirectory { get; set; } = "C:\\temp";
        public string OutputFilesDirectory { get; set; } = "C:\\temp";

        public string SourceFilePath => Path.Combine(SourceFileDirectory, SourceFileName);
        public string OutputSplitFilesDirectory => Path.Combine(OutputFilesDirectory, SplitFilesSubDirectory);
    }
}
