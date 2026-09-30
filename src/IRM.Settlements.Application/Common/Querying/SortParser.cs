namespace IRM.Settlements.Application.Common.Querying;

public static class SortParser
{
    public static List<SortItem> Parse(string? sort, HashSet<string> allowedFields)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return [];

        var result = new List<SortItem>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var parts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var raw in parts)
        {
            var value = raw.Trim();

            if (value.Length == 0)
                continue;

            var isDesc = value[0] == '-';

            var field = isDesc
                ? value[1..]
                : value;

            if (string.IsNullOrWhiteSpace(field))
                continue;

            // нормализация
            field = field.Trim();

            // whitelist
            if (!allowedFields.Contains(field))
                continue;

            // убираем дубликаты (берём первый)
            if (!seen.Add(field))
                continue;

            result.Add(new SortItem(
                field,
                isDesc ? SortDirection.Desc : SortDirection.Asc));
        }

        return result;
    }
}
