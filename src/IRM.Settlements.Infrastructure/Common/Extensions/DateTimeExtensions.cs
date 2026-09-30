using System.Globalization;

namespace IRM.Settlements.Infrastructure.Common.Extensions;

public static class DateTimeExtensions
{
    // Kafka не должна содержать локальное время без timezone.
    // Это не совместимо с распределёнными системами.
    // Либо UTC, либо ISO8601 с offset.
    // Иначе мы будем угадывать данные, а это уже не инженерия.
    public static DateTime MskToUtcMaybe(this DateTime date)
    {
        TimeZoneInfo tz;

        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        }
        catch (TimeZoneNotFoundException)
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        }

        return TimeZoneInfo.ConvertTimeToUtc(date, tz);
    }

    /// <summary>
    ///     Вернет название месяца в родительном падеже
    /// </summary>
    /// <param name="date">дата</param>
    /// <returns></returns>
    public static string GetMonthGenitiveName(this DateOnly? date)
    {
        if (date is null)
            return string.Empty;

        return new CultureInfo("ru-RU").DateTimeFormat.MonthGenitiveNames.Select(x => x.ToLower()).ToArray()
            [date.Value.Month - 1];
    }
}
