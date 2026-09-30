using IRM.Settlements.Application.Abstractions.Excel;
using IRM.Settlements.Application.Abstractions.IrmCatalog;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Extensions;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.Reports.Queries;

public record ExportReportQuery(Guid ReportId)
{
    public static class ExportReportQueryHandler
    {
        public static async Task<FileResult> Handle(
            ExportReportQuery query,
            IPermissionsService permissionsService,
            IIrmCatalogService catalogService,
            IReportRepository reportReadonlyRepository,
            IExcelExporter excelExporter,
            CancellationToken ct)
        {
            var report = await reportReadonlyRepository.GetReportWithItemsAsync(query.ReportId, ct);
            permissionsService.CheckPermissionOrThrow(PermissionTypes.ReportExport, report);

            var serviceCompany = await catalogService.GetServiceCompanyDetailsAsync(report.ServiceCompanySapId, ct);
            if (serviceCompany == null)
                throw new NotFoundException($"ServiceCompany {report.ServiceCompanySapId} not found");

            var bytes = excelExporter.ReportToXls(report, serviceCompany);

            var reportName = $"Отчет_Установка_техники_{report.ServiceCompanyName}_" +
                             $"{report.ServiceDateFrom:dd-MM-yyyy}-{report.ServiceDateTo:dd-MM-yyyy}.xlsx";

            return new FileResult
            {
                FileName = reportName.ToValidFileName(),
                Data = bytes,
                ContentType = Constants.ContentTypes.Excel
            };
        }
    }
}
