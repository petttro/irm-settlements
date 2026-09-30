namespace IRM.Settlements.Application.Abstractions.Repositories;

public interface IReportNumberGenerator
{
    Task<int> NextAsync(int year, CancellationToken ct);
}
