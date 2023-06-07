namespace Sorter.BigFiles.Generator
{
    internal static class StartupValidator
    {
        internal static void Validate(ConfigOptions configOptions)
        {
            if (!File.Exists(configOptions.InputStringsFilePath))
            {
                Console.WriteLine("Provided input file is not exist");
                throw new FileNotFoundException();
            }

            if (!Directory.Exists(configOptions.OutputFilesDirectory))
                Directory.CreateDirectory(configOptions.OutputFilesDirectory);
        }
    }
}
