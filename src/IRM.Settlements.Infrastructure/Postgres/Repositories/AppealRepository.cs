using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.QueryFilters;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Infrastructure.Postgres.Repositories;

/// <summary>
/// Репозиторий для работы с сущностью Appeal.
/// </summary>
public sealed class AppealRepository : Repository<Appeal, int>, IAppealRepository
{
    private readonly ILogger<AppealRepository> _logger;

    /// <summary>
    /// Конструктор репозитория.
    /// </summary>
    /// <param name="context">Контекст базы данных Settlement.</param>
    /// <param name="logger">Логгер</param>
    public AppealRepository(SettlementsDbContext context, ILogger<AppealRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    public override async Task<Appeal?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await QueryWithIncludes(
                a => a.ServiceCompany,
                a => a.ServiceCenter)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task SetReportIdsAsync(HashSet<string> couponNumbers, Guid? reportId, CancellationToken cancellationToken)
    {
        await DbSet
            .Where(x => couponNumbers.Contains(x.CouponNumber))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ReportId, reportId), cancellationToken);
    }

    public async Task<List<Appeal>> GetListAsync(AppealsFilter filter, CancellationToken cancellationToken)
    {
        var query = QueryWithIncludes(
                a => a.ServiceCompany,
                a => a.ServiceCenter)
            .AsNoTracking()
            .Where(a => a.ServiceCompany.SapId == filter.ServiceCompanySapId)
            .Where(a => a.ServiceDate >= filter.ServiceDate.From && a.ServiceDate <= filter.ServiceDate.To);

        if (!filter.IncludeAddedToReport)
            query = query.Where(a => a.ReportId == null);

        if (filter.ServiceCenterExternalIds.Count != 0)
            query = query.Where(a => filter.ServiceCenterExternalIds.Contains(a.ServiceCenter.ExternalId));

        return await query.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Appeal>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public override async Task UpsertAsync(Appeal appeal, CancellationToken cancellationToken = default)
    {
        var existingAppeal = await GetByIdAsync(appeal.Id, cancellationToken);
        if (existingAppeal == null)
        {
            await DbSet.AddAsync(appeal, cancellationToken);
            return;
        }

        // Обновляем только конкретные поля, чтобы не перетирать значения добавленные сервисом
        var updated = await DbSet
            .Where(x => x.Id == appeal.Id && x.UpdatedAt < appeal.UpdatedAt)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.UpdatedAt, appeal.UpdatedAt)
                    .SetProperty(x => x.CouponNumber, appeal.CouponNumber)
                    .SetProperty(x => x.SaleOrderNumber, appeal.SaleOrderNumber)
                    .SetProperty(x => x.OrderNumber, appeal.OrderNumber)
                    .SetProperty(x => x.Created, appeal.Created)
                    .SetProperty(x => x.SaleDate, appeal.SaleDate)
                    .SetProperty(x => x.ServiceDate, appeal.ServiceDate)
                    .SetProperty(x => x.ServiceCompanyId, appeal.ServiceCompanyId)
                    .SetProperty(x => x.ServiceCenterId, appeal.ServiceCenterId)
                    .SetProperty(x => x.ServiceName, appeal.ServiceName)
                    .SetProperty(x => x.ServicePrice, appeal.ServicePrice)
                    .SetProperty(x => x.CityKisId, appeal.CityKisId)
                    .SetProperty(x => x.CityName, appeal.CityName)
                    .SetProperty(x => x.ShopName, appeal.ShopName)
                    .SetProperty(x => x.BrandId, appeal.BrandId)
                    .SetProperty(x => x.WareCode, appeal.WareCode)
                    .SetProperty(x => x.BsiStatus, appeal.BsiStatus)
                    .SetProperty(x => x.PaymentStatus, appeal.PaymentStatus)
                    .SetProperty(x => x.ConfirmType, appeal.ConfirmType)
                    .SetProperty(x => x.ProjectTypeId, appeal.ProjectTypeId)
                    .SetProperty(x => x.CheckNumber, appeal.CheckNumber)
                    .SetProperty(x => x.AdditionalServices, appeal.AdditionalServices),
                cancellationToken
            );

        if (updated == 0 && _logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Appeal: {Id} was not updated. Has newer version in DB.", appeal.Id);
    }
}
