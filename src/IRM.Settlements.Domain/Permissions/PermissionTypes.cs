namespace IRM.Settlements.Domain.Permissions;

public static class PermissionTypes
{
    public const string ReportReadAll = "report.read.all";
    public const string ReportRead = "report.read";
    public const string ReportCreate = "report.create";
    public const string ReportWrite = "report.write";
    public const string ReportDelete = "report.delete";
    public const string ReportRecalculate = "report.recalculate";
    public const string ReportSendToPayment = "report.send-to-payment";
    public const string ReportConfirmPayment = "report.confirm-payment";
    public const string ReportExportRegistry = "report.export.registry";
    public const string ReportExport = "report.export";
    public const string ServiceCenterRead = "service-center.read";
    public const string ServiceCompanyReadAll = "service-company.read.all";
    public const string MvzWrite =  "mvz.write";
    public const string MvzReadAll =  "mvz.read.all";
    public const string PaymentOrderExport = "payment-order.export";
}
