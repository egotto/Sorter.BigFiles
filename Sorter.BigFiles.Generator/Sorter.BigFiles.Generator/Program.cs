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
    $"\tOutput file path: {configOptions.OutputFilesDirectory}{Environment.NewLine}" +
    $"\tInput file with strings path: {configOptions.InputStingsFilePath}{Environment.NewLine}" +
    $"\tOutput file expected size: {configOptions.OutputFileSizeInMBs} MB{Environment.NewLine}" +
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

Environment.Exit(0);