using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.ValueObjects;

namespace IRM.Settlements.Application.Abstractions.Excel;

public interface IExcelExporter
{
    byte[] ReportListToXls(List<Report> rows);

    byte[] ReportToXls(Report report, ServiceCompanyDetails serviceCompany);

    byte[] PaymentOrderToXls(List<PaymentOrderItem> paymentOrderItems);
}
