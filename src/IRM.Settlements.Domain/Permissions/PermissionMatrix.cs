using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.Domain.Permissions;

public static class PermissionMatrix
{
    private static readonly Func<PermissionContext, bool> All = _ => true;
    private static readonly Func<PermissionContext, bool> Own = ctx => ctx.IsOwn;

    public static readonly PermissionMatrixRule[] Rules =
    [
        // DRAFT
        ..CreateRules(
            [IrmRoles.Administrator, IrmRoles.SeniorManager, IrmRoles.CentralOffice],
            [ReportStatus.Draft],
            All,
            [
                PermissionTypes.ReportReadAll,
                PermissionTypes.ReportRead,
                PermissionTypes.ReportDelete,
                PermissionTypes.ReportRecalculate,
                PermissionTypes.ReportExport,
                PermissionTypes.ReportExportRegistry,
                PermissionTypes.ServiceCenterRead,
                PermissionTypes.ServiceCompanyReadAll,
                PermissionTypes.MvzReadAll,
                PermissionTypes.MvzWrite
            ]),

        ..CreateRules(
            [IrmRoles.ServiceCompany],
            [ReportStatus.Draft],
            Own,
            [
                PermissionTypes.ReportRead,
                PermissionTypes.ReportCreate,
                PermissionTypes.ReportWrite,
                PermissionTypes.ReportDelete,
                PermissionTypes.ReportRecalculate,
                PermissionTypes.ReportSendToPayment,
                PermissionTypes.ReportExport,
                PermissionTypes.ReportExportRegistry,
                PermissionTypes.ServiceCenterRead
            ]),

        // SENT TO PAYMENT
        ..CreateRules(
            [IrmRoles.Administrator, IrmRoles.SeniorManager, IrmRoles.CentralOffice],
            [ReportStatus.SentToPayment],
            All,
            [
                PermissionTypes.ReportReadAll,
                PermissionTypes.ReportRead,
                PermissionTypes.ReportExport,
                PermissionTypes.ReportExportRegistry,
                PermissionTypes.ServiceCenterRead,
                PermissionTypes.ServiceCompanyReadAll,
                PermissionTypes.MvzReadAll,
                PermissionTypes.MvzWrite,
                PermissionTypes.PaymentOrderExport,
                PermissionTypes.ReportConfirmPayment
            ]),

        // PAID
        ..CreateRules(
            [IrmRoles.Administrator, IrmRoles.SeniorManager, IrmRoles.CentralOffice],
            [ReportStatus.Paid],
            All,
            [
                PermissionTypes.ReportReadAll,
                PermissionTypes.ReportRead,
                PermissionTypes.ReportExport,
                PermissionTypes.ReportExportRegistry,
                PermissionTypes.ReportSendToPayment,
                PermissionTypes.ServiceCenterRead,
                PermissionTypes.ServiceCompanyReadAll,
                PermissionTypes.MvzReadAll,
                PermissionTypes.MvzWrite
            ]),

        ..CreateRules(
            [IrmRoles.ServiceCompany],
            [ReportStatus.SentToPayment, ReportStatus.Paid],
            Own,
            [
                PermissionTypes.ReportRead,
                PermissionTypes.ReportExport,
                PermissionTypes.ReportExportRegistry,
                PermissionTypes.ServiceCenterRead
            ]),

        ..CreateRules(
            [IrmRoles.Integration],
            [ReportStatus.SentToPayment, ReportStatus.Paid, ReportStatus.Draft],
            All,
            [
                PermissionTypes.ReportRead
            ])
    ];

    private static IEnumerable<PermissionMatrixRule> CreateRules(
        IrmRoles[] roles,
        ReportStatus[] statuses,
        Func<PermissionContext, bool> condition,
        string[] permissions)
    {
        return roles.SelectMany(role =>
            statuses.Select(status =>
                new PermissionMatrixRule(role, status, condition, permissions)));
    }
}
