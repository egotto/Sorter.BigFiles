using System.Text;
using HPCsharp;

namespace Sorter.BigFiles.App
{
    internal class SortedFileMerger
    {
        private readonly ConfigOptions _options;
        public const string sortedKey = "sorted";
        public const string mergedKey = "merged";

        public SortedFileMerger(ConfigOptions configOptions)
        {
            _options = configOptions;
        }

        public void MergeFiles()
        {
            var files = Directory.GetFiles(_options.OutputSplitFilesDirectory).Where(_ => _.Contains(sortedKey)).ToArray();
            var batchSize = CalculateBatchSize(files.Length);

            for(int i=0;i<files.Length;i+=batchSize)
            {
                var toBeMerging = files.Skip(i).Take(batchSize).ToArray();
                var t = new Thread(new ParameterizedThreadStart(MergeSelectedFilesIntoOne));
                t.Start(toBeMerging);
                Thread.Sleep(100);
            }

            while (Directory.GetFiles(Path.Combine(Path.Combine(_options.OutputSplitFilesDirectory, mergedKey))).Count() < files.Length / batchSize)
            {
                Thread.Sleep(100);
            }
        }

        public void MergeSelectedFilesIntoOne(object? filesArray)
        {
            var files = filesArray as string[] ?? Array.Empty<string>();
            SemStaticPool.SemaphoreFileReader.WaitOne();
            Console.WriteLine($"Started merging files");
            var filesCount = files.Length;
            var mergedFiles = 0;
            Console.WriteLine($"Sorted files merging started. Total files to merge: {filesCount}.");
            while (mergedFiles < filesCount)
            {
                var mergedDirectory = Path.Combine(Path.Combine(_options.OutputSplitFilesDirectory, mergedKey));
                if(!Directory.Exists(mergedDirectory))
                    Directory.CreateDirectory(mergedDirectory);
                    
                var fileName = Path.Combine(mergedDirectory, Path.GetFileName(files[0]).Replace(sortedKey, mergedKey));
                if (File.Exists(fileName))
                    File.Delete(fileName);

                var readers = new SortingReader[files.Length];
                for (int i = 0; i < files.Length; i++)
                {
                    readers[i] = new SortingReader(files[i], i);
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
                
            }
            SemStaticPool.SemaphoreFileReader.Release();
            Console.WriteLine($"End merging files");
        }

        private int CalculateBatchSize(int filesCount)
        {
            var processorCount = Environment.ProcessorCount;
            var batchSize = 3;
            while(filesCount % batchSize != 0)
            {
                batchSize = batchSize < 5 ? batchSize + 1 : 2;
            }

            return batchSize;
        }
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
