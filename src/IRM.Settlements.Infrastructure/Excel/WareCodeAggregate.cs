namespace IRM.Settlements.Infrastructure.Excel;

public record WareCodeAggregate
{
    public required string WareCode { get; init; }
    public required string ServiceName { get; init; }
    public required int Count { get; init; }
    public required decimal Cost { get; init; }
}
