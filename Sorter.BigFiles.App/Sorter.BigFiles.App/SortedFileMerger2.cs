using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter.BigFiles.App
{

    internal class SortedFileMerger2
    {
        private readonly ConfigOptions _options;

        public SortedFileMerger2(ConfigOptions configOptions)
        {
            _options = configOptions;
        }

        public string[] mergeSplit(string[] files)
        {
            if(files.Length <= 1)
                return files;
            
            var leftSize = files.Length / 2;
            var rightSize = files.Length - leftSize;
            string[] left = new string[leftSize];
            string[] right = new string[rightSize];
            Array.Copy(files, 0, left, 0, leftSize);
            Array.Copy(files, leftSize, right, 0, rightSize);

            return merge(mergeSplit(left), mergeSplit(right));
        }

        public string[] merge(string[] left, string[] right)
        {
            return MergeSelectedFilesIntoOne(left.Concat(right).ToArray());
        }

        public string MergeFiles()
        {
            var files = Directory
                .GetFiles(_options.OutputSplitFilesDirectory)
                .Where(_ => _.Contains(StaticValues.SortedKey))
                .ToArray();

            var resultFile = mergeSplit(files);

            if(resultFile.Length == 1)
                return resultFile[0];
            
            throw new Exception();
        }


        public string[] MergeSelectedFilesIntoOne(string[] files)
        {
            // SemStaticPool.SemaphoreFileReader.WaitOne();
            Console.WriteLine($"Started merging files");
            var filesCount = files.Length;
            Console.WriteLine($"Sorted files merging started. Total files to merge: {filesCount}.");
                
            var fileName = Path.Combine(_options.OutputSplitFilesDirectory, Guid.NewGuid().ToString()+".txt");
            if (File.Exists(fileName))
                File.Delete(fileName);

            var readers = new SortingReader[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                readers[i] = new SortingReader(files[i], i);
            }

            var sb = new StringBuilder();
            long addedLinesCount = 0;

            while (readers.Any(_=>_.Value != null))
            {
                var vals = readers
                    .Where(_ => _.Value != null)
                    .Select(_ => new{v=_.Value,i=_.Index})
                    .ToArray();
                Array.Sort(vals, (a,obj)=>a.v.CompareTo(obj.v));
                var val = vals.First();                        

                AppendLineToFile(val.v.ToString());
                readers.First(_ => _.Index == val.i).ReadNext();
            }

            File.AppendAllText(fileName, sb.ToString());
            
            foreach(var r in readers)
            {
                if(r.Value != null)
                {
                    var fName = ((FileStream)r.Reader.BaseStream).Name;
                    r.Reader.Close();
                    throw new AggregateException($"File {fName} was not read till the end...");
                }
            }

            void AppendLineToFile(string line)
            {
                sb.AppendLine(line);
                addedLinesCount++;
                if(addedLinesCount > StaticValues.AverageLinesCountPerFile)
                {
                    File.AppendAllText(fileName, sb.ToString());
                    sb.Clear();
                    addedLinesCount = 0;
                }
            }
            
            // SemStaticPool.SemaphoreFileReader.Release();
            Console.WriteLine($"End merging files");

            return new []{fileName};
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
                File.Delete(fName);
            }
        }

        public void Dispose()
        {
            Reader.Dispose();
        }
    }
}