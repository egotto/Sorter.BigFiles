namespace Sorter.BigFiles.Generator
{
    public class ConfigOptions
    {
        public static string SectionName = "Config";

        public string OutputFilesDirectory { get; set; } = string.Empty;
        public string OutputFileName { get; set; } = string.Empty;        
        public int OutputFileSizeInMBs { get; set; } = 100;
        public string OutputStringFormat { get; set; } = string.Empty;

        public string InputStingsFileName { get; set; } = string.Empty;

        public string InputStringsFilePath => Path.Combine(Directory.GetCurrentDirectory(), this.InputStingsFileName);
        public long OutputFileSizeInBytes => OutputFileSizeInMBs * 1024 * 1024;
    }

}
