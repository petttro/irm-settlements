using IRM.Settlements.Application.Abstractions.Excel;
using IRM.Settlements.Application.Abstractions.IrmCatalog;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Extensions;
using IRM.Settlements.Application.QueryFilters;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.Permissions;

namespace IRM.Settlements.Application.UseCases.PaymentOrders;

public record ExportPaymentOrderQuery(List<Guid> ReportIds)
{
    public static class ExportPaymentOrderQueryHandler
    {
        private const int RowsLimit = 998;
        private const string NdsMaterialNumber = "990000732";
        private const string NonNdsMaterialNumber = "990001077";

        public static async Task<FileResult> Handle(
            ExportPaymentOrderQuery query,
            IPermissionsService permissionsService,
            IIrmCatalogService irmCatalogService,
            IReportRepository reportRepository,
            IExcelExporter excelExporter,
            CancellationToken ct)
        {
            var reportsFilter = new ReportsFilter { ReportIds = query.ReportIds };
            var reports = await reportRepository.ReadOnlyListAsync(reportsFilter, paging: null, sortItems: [], ct);
            foreach (var report in reports)
            {
                permissionsService.CheckPermissionOrThrow(PermissionTypes.PaymentOrderExport, report);
            }

            var distinctSapIds = reports.Select(r => r.ServiceCompanySapId).Distinct().ToList();
            var serviceCompaniesDetails = await irmCatalogService.GetServiceCompaniesDetailsAsync(distinctSapIds, ct);
            var serviceCompaniesMap = serviceCompaniesDetails.ToDictionary(sc => sc.SapId, sc => sc);

            var invoiceDate = GetInvoiceDate();
            var data = await reportRepository.GetPaymentOrderDataAsync(query.ReportIds, ct);

            var paymentOrder = data
                .Select(groupDataItem => new PaymentOrderItem
                {
                    ServiceCompanySapId = groupDataItem.ServiceCompanySapId,
                    ReportNumber = groupDataItem.ReportNumber,
                    Mvz = groupDataItem.MvzCode,
                    ShopName = groupDataItem.ShopName,
                    SapContractNumber = serviceCompaniesMap[groupDataItem.ServiceCompanySapId].SapContractNumber,
                    InvoiceDate = invoiceDate,
                    MaterialNumber = int.TryParse(groupDataItem.SapNds, out _)
                        ? NdsMaterialNumber
                        : NonNdsMaterialNumber,
                    TotalCost = groupDataItem.TotalCost
                })
                .OrderBy(x => x.ServiceCompanySapId)
                .ThenBy(x => x.ReportNumber)
                .ThenBy(x => x.Mvz == null)
                .ThenBy(x => x.Mvz)
                .ThenBy(x => x.ShopName)
                .ToList();

            if (paymentOrder.Count > RowsLimit)
                throw new ReportIncorrectStateException($"Превышен допустимый лимит экспортируемых строк: {RowsLimit}");

            var bytes = excelExporter.PaymentOrderToXls(paymentOrder);
            var reportName = $"Заказ_на_Оплату_Установка_техники_{invoiceDate:dd-MM-yyyy}.xlsx";

            return new FileResult
            {
                FileName = reportName.ToValidFileName(),
                Data = bytes,
                ContentType = Constants.ContentTypes.Excel
            };
        }

        private static DateOnly GetInvoiceDate()
        {
            var today = DateTime.Today;

            if (today.Day > 25)
                return DateOnly.FromDateTime(today);

            return new DateOnly(today.Year, today.Month, 1)
                .AddDays(-1);
        }
    }
}
