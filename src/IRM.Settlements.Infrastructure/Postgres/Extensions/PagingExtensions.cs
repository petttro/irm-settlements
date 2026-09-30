namespace IRM.Settlements.Infrastructure.Postgres.Extensions;

public static class PagingExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int pageIndex, int pageSize)
    {
        pageIndex = pageIndex < 0 ? 0 : pageIndex;
        pageSize = pageSize is < 1 or > 1000 ? 20 : pageSize;

        return query
            .Skip(pageIndex * pageSize)
            .Take(pageSize);
    }
}
