namespace Sorter.BigFiles.App.Models
{
    internal class SortingReader : IDisposable
    {
        private const string _separator = ". ";
        public SortingReader(string filePath, int index)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            Index = index;
            Reader = new StreamReader(filePath);

            ReadNext();
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