namespace IRM.Settlements.Application.Common;

public record DateOnlyRange(DateOnly? From, DateOnly? To)
{
    public bool HasValues => From.HasValue || To.HasValue;
}
