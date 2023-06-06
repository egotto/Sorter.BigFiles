﻿namespace Sorter.BigFiles.App
{
    internal class SortLine : IComparable
    {
        public SortLine(string[] line)
        {
            if (line.Length != 2)
                throw new ArgumentException(string.Format("Invalid parameters: {0}", line.Length));

            if (!int.TryParse(line[0], out var number))
                throw new ArgumentException("Invalid line structure");

            Number = number;
            Text = line[1];
        }

        public int Number { get; set; }

        public string Text { get; set; }

        public int CompareTo(object? obj)
        {
            if (obj is not SortLine)
                throw new ArgumentException("Invalid parameters");

            var result = Text.CompareTo(((SortLine)obj).Text);
            if (result != 0) 
                return result;
            else
                return Number.CompareTo(((SortLine)obj).Number);
        }

        public override bool Equals(object? obj)
        {
            switch (obj)
            {
                case not SortLine:
                    throw new ArgumentException("Invalid parameters");
                case SortLine line:
                default:
                    return Text.Equals(line.Text) &&
                        Number.Equals(line.Number);
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Number, Text);
        }

        public override string ToString()
        {
            return string.Format("{0}. {1}", Number, Text);
        }
    }
}
