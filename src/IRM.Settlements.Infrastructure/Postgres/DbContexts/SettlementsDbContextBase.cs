using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Infrastructure.Postgres.Configurations;
using Microsoft.EntityFrameworkCore;

namespace IRM.Settlements.Infrastructure.Postgres.DbContexts;

/// <summary>
/// Базовый контекст для создания контекстов rw,ro.
/// </summary>
public abstract class SettlementsDbContextBase : DbContext
{
    public virtual DbSet<Appeal> Appeals { get; set; }

    public virtual DbSet<ServiceCenter> ServiceCenters { get; set; }

    public virtual DbSet<ServiceCompany> ServiceCompanies { get; set; }

    public virtual DbSet<MvzItem> Mvz { get; set; }

    protected SettlementsDbContextBase(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfiguration(new AppealConfiguration())
            .ApplyConfiguration(new ServiceCompanyConfiguration())
            .ApplyConfiguration(new ServiceCenterConfiguration())
            .ApplyConfiguration(new ReportConfiguration())
            .ApplyConfiguration(new ReportItemConfiguration())
            .ApplyConfiguration(new MvzConfiguration())
            .HasDefaultSchema("settlements");
    }
}
