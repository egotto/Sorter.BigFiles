namespace Sorter.BigFiles.App.Models;

internal class SortLine : SortLineBase, ISortLine<SortLine>, IHaveSortableStructure
{
    public SortLine(string line) : base(line)
    {
        if (string.IsNullOrEmpty(line))
            throw new ArgumentNullException(nameof(line));

        var lines = line.Split(IHaveSortableStructure.SplitSeparator);

        if (lines.Length != 2)
            throw new ArgumentException(string.Format("Invalid parameters: {0}", lines.Length));

        if (!int.TryParse(lines[0], out var number))
            throw new ArgumentException("Invalid line structure");

        Number = number;
        Text = lines[1];
    }

    public int Number { get; init; }

    public string Text { get; init; }

    public int CompareTo(SortLine? other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        var result = Text.CompareTo(((SortLine)other).Text);
        if (result != 0)
            return result;
        else
            return Number.CompareTo(((SortLine)other).Number);
    }

    public string SerializeToString()
    {
        return string.Format("{0}. {1}", Number, Text);
    }

    public bool Equals(SortLine? other)
    {
        switch (other)
        {
            case null:
                throw new ArgumentNullException(nameof(other));
            case SortLine line:
            default:
                return Text.Equals(line.Text) &&
                    Number.Equals(line.Number);
        }
    }
}