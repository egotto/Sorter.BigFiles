namespace Sorter.BigFiles.App
{
    public class ConfigOptions
    {
        public bool FileSplitEnabled { get; set; } = true;
        public bool SplitSortingEnabled { get; set; } = true;
        public long MaxFileSizeInMBs { get; set; } = 50;
        public string SplitFilesSubDirectory { get; set; } = "split";
        public string OutputFilesTemplate { get; set; } = "document_{i}.txt";
        public string SourceFileName { get; set; } = "file2sort1.txt";
        public string SourceFileDirectory { get; set; } = "/home/evgenij/repos/temp";
        public string OutputFilesDirectory { get; set; } = "/home/evgenij/repos/temp";

        public string SourceFilePath => Path.Combine(SourceFileDirectory, SourceFileName);
        public string OutputSplitFilesDirectory => Path.Combine(OutputFilesDirectory, SplitFilesSubDirectory);
    }
}
