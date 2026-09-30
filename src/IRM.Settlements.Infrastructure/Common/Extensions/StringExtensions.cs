namespace IRM.Settlements.Infrastructure.Common.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || maxLength <= 0)
            return string.Empty;

        return value.Length <= maxLength
            ? value
            : string.Concat(value.AsSpan(0, maxLength), "... TRUNCATED");
    }
}
