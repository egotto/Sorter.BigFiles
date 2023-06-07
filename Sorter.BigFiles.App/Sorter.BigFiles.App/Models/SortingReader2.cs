namespace Sorter.BigFiles.App.Models
{
    internal class SortingReader2
    {
        private readonly IEnumerator<string> _lines;
        private readonly string _filePath;
        public SortingReader2(string filePath, int index)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            Index = index;
            _lines = File.ReadLines(filePath).GetEnumerator();
            _filePath = filePath;

            ReadNext();
        }

        public SortLine? Value { get; set; }

        public int Index { get; set; }

        public void ReadNext()
        {
            if (_lines.MoveNext())
            {
                Value = new SortLine(_lines.Current.Split(StaticValues.LineSplitSeparator));
            }
            else
            {
                Value = null;
                File.Delete(_filePath);
            }
        }
    }
}