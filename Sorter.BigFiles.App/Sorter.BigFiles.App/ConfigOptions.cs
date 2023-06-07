namespace Sorter.BigFiles.App;

public class ConfigOptions
{
    public static string SectionName = "Config";

    /// <summary>
    /// Debug only
    /// </summary>
    public bool FileSplitEnabled { get; set; } = true;

    /// <summary>
    /// Debug only
    /// </summary>
    public bool SplitSortingEnabled { get; set; } = true;

    /// <summary>
    /// Limiter for the amount of memory used.
    /// </summary>
    public long AvailableRamInMBs { get; set; } = 5000;

    /// <summary>
    /// If the value is set to 0, all available cores on the system will be used.
    /// </summary>
    public int AvailableCores { get; set; } = 0;

    /// <summary>
    /// SubDirectory for temporary and a final result files.
    /// </summary>
    public string SplitFilesSubDirectory { get; set; } = "split";

    /// <summary>
    /// Filename template for a temporary files.
    /// </summary>
    public string OutputFilesTemplate { get; set; } = "document_{i}.txt";

    /// <summary>
    /// Source file name.
    /// </summary>
    public string SourceFileName { get; set; } = "file2sort10.txt";

    /// <summary>
    /// Directory with source file.
    /// </summary>
    public string SourceFileDirectory { get; set; } = "C:\\temp";

    /// <summary>
    /// Directory for a temporary and result files
    /// </summary>
    public string OutputFilesDirectory { get; set; } = "C:\\temp";

    /// <summary>
    /// The full path to the source file.
    /// </summary>
    public string SourceFilePath => Path.Combine(SourceFileDirectory, SourceFileName);

    /// <summary>
    /// The full path to the temporary directory
    /// </summary>
    public string OutputSplitFilesDirectory => Path.Combine(OutputFilesDirectory, SplitFilesSubDirectory);
}