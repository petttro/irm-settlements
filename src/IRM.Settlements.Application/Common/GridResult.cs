namespace IRM.Settlements.Application.Common;

public class GridResult<T>
{
    public List<T> Data { get; set; } = [];

    public PaginatedQuery PaginatedQuery { get; set; } = new();

    public int TotalSize { get; set; }
}

public class PaginatedQuery
{
    public int PageIndex { get; set; }

    public int PageSize { get; set; }
}
