namespace Sorter.BigFiles.Generator
{
    public class ConfigOptions
    {
        public static string SectionName = "Config";

        public string OutputFilesDirectory { get; set; } = string.Empty;
        public string InputStingsFilePath { get; set; } = string.Empty;
        public int OutputFileSizeInMBs { get; set; } = 100;
    }

}
