using Microsoft.Extensions.Configuration;
using Sorter.BigFiles.Generator;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

IConfiguration configuration = builder.Build();
var configOptions = configuration.GetSection(ConfigOptions.SectionName).Get<ConfigOptions>() ??
                    throw new ArgumentException("Bad config file");

Console.Write($"File Generator is starting.{Environment.NewLine}" +
    $"Please check configuration:{Environment.NewLine}" +
    $"\tOutput file path: {configOptions.OutputFilesDirectory}{configOptions.OutputFileName}{Environment.NewLine}" +
    $"\tOutput strings format: {configOptions.OutputStringFormat}{Environment.NewLine}" +
    $"\tOutput file expected size: {configOptions.OutputFileSizeInMBs} MB{Environment.NewLine}" +
    $"\tInput file with strings path: {configOptions.InputStingsFileName}{Environment.NewLine}" +
    $"Is provided configuration correct?(Y/n):");

switch (Console.ReadKey().Key)
{
    case ConsoleKey.N:
        Console.WriteLine($"{Environment.NewLine}Please change provided configuration and start app again");
        Environment.Exit(1);
        break;
    case ConsoleKey.Y:
    default:
        break;
}

StartupValidator.Validate(configOptions);
var outputFilePath = Path.Combine(configOptions.OutputFilesDirectory, configOptions.OutputFileName);
var inputFileReader = new InputStringsFileReader();
var lineGenerator = new StringLineGenerator(configOptions.OutputStringFormat, inputFileReader.ReadInputStrings(configOptions.InputStringsFilePath));

var fileGenerator = new BigFileGenerator(outputFilePath, configOptions.OutputFileSizeInBytes, lineGenerator);
fileGenerator.CreateFileWithStrings(out var addedLinesCount);
Console.WriteLine($"The expected file has been created and saved to the path: {outputFilePath}.{Environment.NewLine}File contains {addedLinesCount:N} lines of strings.");

Environment.Exit(0);