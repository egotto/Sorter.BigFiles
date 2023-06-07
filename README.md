# Introduction
These are scripts for sorting files. For large files of 10 GB or more.
Here we have two applications - Generator and App. 

# Requirements  
1. A machine with a minimum of 8 GB of RAM and 2 CPUs is recommended to run. The storage speed directly affects the script execution speed.
2. Dotnet SDK (7+ version)
3. Generated (or downloaded) source file with at least double free space (for splitted files and for output file).

# Generator
The Generator application is a helper application and is not intended to be used frequently. This script generates files of given length from random strings and numbers. The lines are not completely random, in the directory with the source files there is a file "data.txt" in which line by line options of lines are presented that will be used when generating the output file. You can use the suggested file or replace it with your own.
The following fields are also available in the configuration file:
- **Config** - general settings
  - _OutputFilesDirectory_ - directory with output and temporary files after processing (will be created if it does not exist);
  - _OutputFileName_ - file name for a generated file;
  - _OutputFileSizeInMBs_ - expected file size in MBs;
  - _OutputStringFormat_ - string format for an output file. Should contain 2 values and separator. For example default value is `"{0}. {1}"`. Here `{0}` is a Number and `{1}` is a string. So output line will be `Number. String`;
  - _InputStingsFileName_ - file with random strings that will be used for generating output file. Should contains any string lines line by line.


# App
The App is a sorter. The input is a file with random lines, unordered. The advantage is that this algorithm is not strictly suitable for any particular string format. It can be replaced.
In the configuration file, you can (and should before the first run) configure the following settings:
- **Config** - general settings
  - _AvailableRamInMBs_ - limiter for the amount of memory used (used for calculation when splitting source file);
  - _AvailableCores_ - if the value is set to 0, all available cores on the system will be used;
  - _SplitFilesSubDirectory_ - subDirectory for temporary and a final result files;
  - _SourceFileName_ - source file name that will be sorted;
  - _SourceFileDirectory_ - directory with the source file
  - _OutputFilesDirectory_ - directory with output and temporary files after processing (will be created if it does not exist).


## Structure and principle of work.
The algorithm works as follows. 
1. The file is split into several smaller files that can fit in memory. At least two files to speed up in-memory processing using multithreading. On smaller files, this slows things down a little, but on larger files, it's an advantage.  
2. Next, the created files are sorted in memory in parallel streams.  
3. When all files are sorted, the process of merging the sorted files begins. The files are combined in groups of three files. Multiple measurements and tests have shown that this number is much more efficient than pairing. In memory, of course, recursion would work better with a pairwise union, but here the file system is used.  

# Getting Started
The launch is best done after building the application in the release configuration for the system in which the processing will be performed:  
`dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -o <output directory> --self-contained false`  
or  
`dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -o <output directory> --self-contained false`  
TBD: Dockerfile will be added