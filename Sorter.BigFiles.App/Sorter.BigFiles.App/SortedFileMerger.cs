using System.Text;
using HPCsharp;

namespace Sorter.BigFiles.App
{
    internal class SortedFileMerger
    {
        private readonly ConfigOptions _options;

        public SortedFileMerger(ConfigOptions configOptions)
        {
            _options = configOptions;
        }

        public void MergeFiles()
        {
            var files = Directory.GetFiles(_options.OutputSplitFilesDirectory).Where(_ => _.Contains("sorted")).ToArray();
            MergeSelectedFilesIntoOne(files);
            // foreach (var file in files)
            // {
            //     var t = new Thread(new ParameterizedThreadStart(sorter.SortFile));
            //     t.Start(file);
            //     Thread.Sleep(100);
            // }
        }

        public void MergeSelectedFilesIntoOne(string[] files)
        {
            var filesCount = files.Length;
            var mergedFiles = 0;
            var batchIndex = 0;
            Console.WriteLine($"Sorted files merging started. Total files to merge: {filesCount}.");
            while (mergedFiles < filesCount)
            {
                batchIndex++;
                Console.WriteLine($"Batch #{batchIndex} merging");
                var mergedDirectory = Path.Combine(Path.Combine(_options.OutputSplitFilesDirectory, "merged"));
                if(!Directory.Exists(mergedDirectory))
                    Directory.CreateDirectory(mergedDirectory);
                    
                var fileName = Path.Combine(mergedDirectory, _options.OutputFilesTemplate.Replace("{i}", $"{batchIndex}"));
                if (File.Exists(fileName))
                    File.Delete(fileName);

                var files10 = files.Skip(mergedFiles).Take(4).ToArray();
                var readers = new SortingReader[files10.Length];
                for (int i = 0; i < files10.Length; i++)
                {
                    readers[i] = new SortingReader(files10[i], i);
                }

                var sb = new StringBuilder();
                while (readers.Any(_=>_.Value != null))
                {
                    var vals = readers
                        .Where(_ => _.Value != null)
                        .Select(_ => new{v=_.Value,i=_.Index})
                        .ToArray();
                    Array.Sort(vals, (a,obj)=>a.v.CompareTo(obj.v));
                    var val = vals.First();                        

                    sb.AppendLine(val.v.ToString());
                    readers.First(_ => _.Index == val.i).ReadNext();
                }

                File.WriteAllText(fileName, sb.ToString());
                
                foreach(var r in readers)
                {
                    if(r.Value != null)
                    {
                        var fName = ((FileStream)r.Reader.BaseStream).Name;
                        r.Reader.Close();
                        throw new AggregateException($"File {fName} was not read till the end...");
                    }
                }

                mergedFiles += readers.Length;
                Console.WriteLine($"Batch #{batchIndex} merged");
            }
        }

        // private int CalculateBatchSize(int filesCount)
        // {
        //     var processorCount = Environment.ProcessorCount;
        //     return processorCount / 4;
        // }
    }

    internal class SortingReader : IDisposable
    {
        private const string _separator = ". ";
        public SortingReader(string filePath, int index)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            Reader = new StreamReader(filePath);

            ReadNext();
            Index = index;
        }

        public SortLine? Value { get; set; }

        public StreamReader Reader { get; init; }
        public int Index { get; set; }

        public void ReadNext()
        {
            if (!Reader.EndOfStream)
            {
                Value = new SortLine(Reader.ReadLine().Split(_separator));
            }
            else
            {
                Dispose();
                Value = null;
                var fName = ((FileStream)Reader.BaseStream).Name;
                // File.Delete(fName);
            }
        }

        public void Dispose()
        {
            Reader.Dispose();
        }
    }
}
