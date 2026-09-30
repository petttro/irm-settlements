using Microsoft.EntityFrameworkCore;

namespace IRM.Settlements.Infrastructure.Postgres.DbContexts;

public class SettlementsDbContext : SettlementsDbContextBase
{
    public SettlementsDbContext(DbContextOptions<SettlementsDbContext> options)
        : base(options)
    {
    }
}
