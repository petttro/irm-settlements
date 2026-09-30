namespace IRM.Settlements.Application.Common.Querying;

public sealed class SortItem
{
    public string Field { get; }
    public SortDirection Direction { get; }

    public SortItem(string field, SortDirection direction)
    {
        Field = field;
        Direction = direction;
    }
}
