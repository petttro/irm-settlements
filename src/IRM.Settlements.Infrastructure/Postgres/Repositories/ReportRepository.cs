using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Common.Querying;
using IRM.Settlements.Application.QueryFilters;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.ValueObjects;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using IRM.Settlements.Infrastructure.Postgres.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IRM.Settlements.Infrastructure.Postgres.Repositories;

/// <summary>
/// Репозиторий для работы с сущностью User.
/// </summary>
public sealed class ReportRepository : Repository<Report, Guid>, IReportRepository
{
    /// <summary>
    /// Конструктор репозитория.
    /// </summary>
    /// <param name="context">Контекст базы данных Settlement.</param>
    public ReportRepository(SettlementsDbContext context) : base(context)
    {
    }

    public async Task<Report> GetReportWithItemsAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        var report = await QueryWithIncludes(x => x.Items.OrderBy(i => i.ServiceDate))
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

        if (report == null)
            throw new NotFoundException($"Report {reportId} not found");

        return report;
    }

    public async Task<Report> GetReportByIdAsync(Guid reportId, CancellationToken ct)
    {
        var report = await DbSet.FirstOrDefaultAsync(r => r.Id == reportId, ct);
        if (report == null)
            throw new NotFoundException($"Report {reportId} not found");

        return report;
    }

    public async Task<List<ReportPaymentOrderInfo>> GetPaymentOrderDataAsync(List<Guid> reportIds, CancellationToken cancellationToken)
    {
        var query = Query()
            .AsNoTracking()
            .Where(report => reportIds.Contains(report.Id))
            .SelectMany(report => report.Items)
            .GroupBy(reportItem => new
            {
                reportItem.Report!.ServiceCompanySapId,
                reportItem.Report.Number,
                reportItem.Mvz!.MvzCode,
                reportItem.ShopName,
                reportItem.SapNds
            })
            .Select(group => new ReportPaymentOrderInfo
            {
                ServiceCompanySapId = group.Key.ServiceCompanySapId,
                ReportNumber = group.Key.Number,
                MvzCode = group.Key.MvzCode,
                ShopName = group.Key.ShopName,
                SapNds = group.Key.SapNds,
                TotalCost = group.Sum(x => x.TotalCost)
            });

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Report>> ReadOnlyListAsync(ReportsFilter filter, Paging? paging, IReadOnlyList<SortItem> sortItems,
        CancellationToken cancellationToken)
    {
        var query = BuildReportsQuery(filter).AsNoTracking();

        if (filter.ReportIds.Count > 0)
            query = query.Where(report => filter.ReportIds.Contains(report.Id));

        if (sortItems.Count > 0)
            query = query.ApplySorting(sortItems);
        else
            query = query.ApplySorting([new SortItem(nameof(Report.CreatedAt), SortDirection.Desc)]);

        if (paging != null)
            query = query.ApplyPaging(paging.Page, paging.PageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Report>> ListAsync(ReportsFilter filter, Paging? paging, IReadOnlyList<SortItem> sortItems,
        CancellationToken cancellationToken)
    {
        var query = BuildReportsQuery(filter);

        if (sortItems.Count > 0)
            query = query.ApplySorting(sortItems);

        if (paging != null)
            query = query.ApplyPaging(paging.Page, paging.PageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<ReportItem>> GetReportItemsAsync(Guid reportId, Paging? paging, IReadOnlyList<SortItem> sortItems,
        CancellationToken cancellationToken)
    {
        var query = Query()
            .AsNoTracking()
            .SelectMany(report => report.Items)
            .Where(reportItem => reportItem.ReportId == reportId);

        if (sortItems.Count > 0)
            query = query.ApplySorting(sortItems);
        else
            query = query.OrderBy(i => i.ServiceDate);

        if (paging != null)
            query = query.ApplyPaging(paging.Page, paging.PageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ReportsFilter filter, CancellationToken cancellationToken)
    {
        return await BuildReportsQuery(filter).AsNoTracking().CountAsync(cancellationToken);
    }

    public async Task<int> CountReportItemsAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        var query = Query()
            .AsNoTracking()
            .SelectMany(report => report.Items)
            .Where(reportItem => reportItem.ReportId == reportId);

        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<Report> BuildReportsQuery(ReportsFilter filter)
    {
        var query = Query();

        // системный фильтр
        if (!string.IsNullOrEmpty(filter.ServiceCompanySapId))
            query = query.Where(x => x.ServiceCompanySapId == filter.ServiceCompanySapId);

        // поиск
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(x => x.Items.Any(reportItem => EF.Functions.ILike(reportItem.SearchText, $"%{filter.Search}%")));

        // пользовательские фильтры
        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);

        if (filter.CreatedAt is { HasValues: true })
            query = query
                .Where(report => report.CreatedAt >= filter.CreatedAt.From)
                .Where(report => report.CreatedAt <= filter.CreatedAt.To);

        if (filter.SentToPaymentDate is { HasValues: true })
            query = query
                .Where(report => report.SentToPaymentDate >= filter.SentToPaymentDate.From)
                .Where(report => report.SentToPaymentDate <= filter.SentToPaymentDate.To);

        if (filter.PaymentDate is { HasValues: true })
            query = query
                .Where(report => report.PaymentDate >= filter.PaymentDate.From)
                .Where(report => report.PaymentDate <= filter.PaymentDate.To);

        return query;
    }
}
