namespace Sorter.BigFiles.App
{
    internal class SortLine
    {
        public SortLine(string[] line)
        {
            if (line.Length != 2)
                throw new ArgumentException("Invalid line");

            if (!int.TryParse(line[0], out var number))
                throw new ArgumentException("Invalid line structure");

            Number = number;
            Text = line[1];
        }

        public int Number { get; set; }

        public string Text { get; set; }

        public override string ToString()
        {
            return string.Format("{0}. {1}", Number, Text);
        }
    }
}
