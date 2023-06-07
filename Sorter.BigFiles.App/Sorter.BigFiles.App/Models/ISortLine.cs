namespace Sorter.BigFiles.App.Models;

public abstract class SortLineBase
{
    protected SortLineBase(string line)
    {
    }
}

public interface ISortLine<T> : IEquatable<T>, IComparable<T>
{
    public string SerializeToString();
}

public interface IHaveSortableStructure
{
    public int Number { get; init; }
    public string Text { get; init; }

    public const string SplitSeparator = ". ";
}