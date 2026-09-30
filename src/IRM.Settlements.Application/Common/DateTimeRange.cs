namespace IRM.Settlements.Application.Common;

public record DateTimeRange(DateTime? From, DateTime? To)
{
    public DateTime? From { get; } = From?.ToUniversalTime();
    public DateTime? To { get; } = To?.ToUniversalTime();

    public bool HasValues => From.HasValue || To.HasValue;
}
