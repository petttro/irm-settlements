using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace IRM.Settlements.Infrastructure.Postgres.Repositories;

#pragma warning disable EF1002 // Риск sql инъекции отсутствует
#pragma warning disable S2077

public sealed class ReportNumberGenerator : IReportNumberGenerator
{
    private readonly SettlementsDbContext _dbContext;

    public ReportNumberGenerator(SettlementsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> NextAsync(int year, CancellationToken ct)
    {
        var sequenceName = $"report_{year}_seq";

        await _dbContext.Database.ExecuteSqlRawAsync($"create sequence if not exists {sequenceName} start 1;", ct);

        var next = await _dbContext.Database
            .SqlQueryRaw<long>($"select nextval('{sequenceName}') as \"Value\"")
            .SingleAsync(ct);

        return (int)next;
    }
}
