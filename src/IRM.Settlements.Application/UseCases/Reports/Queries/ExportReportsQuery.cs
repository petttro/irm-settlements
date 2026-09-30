using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Abstractions.Excel;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.QueryFilters;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Queries;

public record ExportReportsQuery(
    string? ServiceCompanySapId,
    ReportStatus? Status,
    DateTimeRange CreatedAt,
    DateTimeRange SentToPaymentDate,
    DateTimeRange PaymentDate) : IServiceCompanyResource
{
    public static class GetReportsQueryHandler
    {
        public static async Task<FileResult> Handle(
            ExportReportsQuery query,
            IReportRepository reportRepository,
            IPermissionsService permissionsService,
            IExcelExporter excelExporter,
            CancellationToken cancellationToken)
        {
            permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportExportRegistry, query);

            var filter = new ReportsFilter(query);
            var reports = await reportRepository.ReadOnlyListAsync(filter, null, [], cancellationToken);
            var bytes = excelExporter.ReportListToXls(reports);

            return new FileResult
            {
                FileName = "Выгрузка по взаиморасчетам.xlsx",
                Data = bytes,
                ContentType = Constants.ContentTypes.Excel
            };
        }
    }
}
