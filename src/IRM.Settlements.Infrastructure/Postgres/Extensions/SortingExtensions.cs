using System.Linq.Expressions;
using System.Reflection;
using IRM.Settlements.Application.Common.Querying;

namespace IRM.Settlements.Infrastructure.Postgres.Extensions;

public static class SortingExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, IReadOnlyList<SortItem> sortItems)
    {
        if (sortItems.Count == 0)
            return query;

        IOrderedQueryable<T>? ordered = null;

        foreach (var item in sortItems)
        {
            ordered = ApplyOrder(query, ordered, item);
        }

        return ordered ?? query;
    }

    private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, IOrderedQueryable<T>? ordered, SortItem sortItem)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var propertyInfo = typeof(T).GetProperty(
            sortItem.Field,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
            throw new ArgumentException($"Property '{sortItem.Field}' not found");

        var property = Expression.Property(parameter, propertyInfo);
        var lambda = Expression.Lambda(property, parameter);

        var isAsc = sortItem.Direction == SortDirection.Asc;
        string methodName;

        if (ordered == null)
            methodName = isAsc ? "OrderBy" : "OrderByDescending";
        else
            methodName = isAsc ? "ThenBy" : "ThenByDescending";

        var result = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), property.Type],
            source.Expression,
            Expression.Quote(lambda));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(result);
    }
}
