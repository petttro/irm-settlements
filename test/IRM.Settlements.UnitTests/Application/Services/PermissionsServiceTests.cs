using IRM.Settlements.Application.Abstractions.Auth;
using IRM.Settlements.Application.Services;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Permissions;
using IRM.Settlements.Domain.ValueObjects;
using Shouldly;
using Moq;

namespace IRM.Settlements.UnitTests.Application.Services;

public class PermissionsServiceTests
{
    private readonly PermissionsService _service;
    private readonly Mock<IUserContext> _userContextMock;

    private const string ServiceCompanySapId = "ServiceCompanySapId";

    private static readonly string[] EmptyPermissions = [];

    private static readonly string[] DraftFullExpectedPermissions =
    [
        PermissionTypes.ReportRead,
        PermissionTypes.ReportWrite,
        PermissionTypes.ReportDelete,
        PermissionTypes.ReportRecalculate,
        PermissionTypes.ReportSendToPayment,
        PermissionTypes.ReportExport,
        PermissionTypes.ServiceCenterRead
    ];

    private static readonly string[] DraftServiceCompanyExpectedPermissions =
    [
        PermissionTypes.ReportRead,
        PermissionTypes.ReportWrite,
        PermissionTypes.ReportDelete,
        PermissionTypes.ReportRecalculate,
        PermissionTypes.ReportSendToPayment,
        PermissionTypes.ReportExport,
        PermissionTypes.ServiceCenterRead
    ];

    private static readonly string[] DraftAdminExpectedPermissions =
    [
        PermissionTypes.ReportRead,
        PermissionTypes.ReportDelete,
        PermissionTypes.ReportRecalculate,
        PermissionTypes.ReportExport,
        PermissionTypes.ServiceCenterRead
    ];

    private static readonly string[] SentToPaymentServiceCompanyExpectedPermissions =
    [
        PermissionTypes.ReportRead,
        PermissionTypes.ReportExport,
        PermissionTypes.ServiceCenterRead
    ];

    private static readonly string[] SentToPaymentAdminExpectedPermissions =
    [
        PermissionTypes.ReportRead,
        PermissionTypes.ReportExport,
        PermissionTypes.ServiceCenterRead,
        PermissionTypes.PaymentOrderExport,
        PermissionTypes.ReportConfirmPayment
    ];

    private static readonly string[] PaidServiceCompanyExpectedPermissions =
    [
        PermissionTypes.ReportRead,
        PermissionTypes.ReportExport,
        PermissionTypes.ServiceCenterRead
    ];

    private static readonly string[] PaidAdminExpectedPermissions =
    [
        PermissionTypes.ReportRead,
        PermissionTypes.ReportExport,
        PermissionTypes.ServiceCenterRead,
        PermissionTypes.ReportSendToPayment
    ];

    public PermissionsServiceTests()
    {
        _userContextMock = new Mock<IUserContext>();
        _service = new PermissionsService(_userContextMock.Object);
    }

    public static IEnumerable<object[]> ScopedPermissionsTestData =>
    [
        [ReportStatus.Draft, new[] { IrmRoles.ServiceCompany, IrmRoles.Administrator }, ServiceCompanySapId, DraftFullExpectedPermissions],
        [ReportStatus.Draft, new[] { IrmRoles.ServiceCompany }, "anotherServiceCompany", EmptyPermissions],

        [ReportStatus.Draft, new[] { IrmRoles.ServiceCompany }, ServiceCompanySapId, DraftServiceCompanyExpectedPermissions],
        [ReportStatus.Draft, new[] { IrmRoles.Administrator }, "anotherServiceCompany", DraftAdminExpectedPermissions],
        [ReportStatus.Draft, new[] { IrmRoles.SeniorManager }, ServiceCompanySapId, DraftAdminExpectedPermissions],
        [ReportStatus.Draft, new[] { IrmRoles.CentralOffice }, "anotherServiceCompany", DraftAdminExpectedPermissions],

        [ReportStatus.SentToPayment, new[] { IrmRoles.ServiceCompany }, ServiceCompanySapId, SentToPaymentServiceCompanyExpectedPermissions],
        [ReportStatus.SentToPayment, new[] { IrmRoles.Administrator }, "anotherServiceCompany", SentToPaymentAdminExpectedPermissions],
        [ReportStatus.SentToPayment, new[] { IrmRoles.SeniorManager }, ServiceCompanySapId, SentToPaymentAdminExpectedPermissions],
        [ReportStatus.SentToPayment, new[] { IrmRoles.CentralOffice }, ServiceCompanySapId, SentToPaymentAdminExpectedPermissions],

        [ReportStatus.Paid, new[] { IrmRoles.ServiceCompany }, ServiceCompanySapId, PaidServiceCompanyExpectedPermissions],
        [ReportStatus.Paid, new[] { IrmRoles.Administrator }, "anotherServiceCompany", PaidAdminExpectedPermissions],
        [ReportStatus.Paid, new[] { IrmRoles.SeniorManager }, ServiceCompanySapId, PaidAdminExpectedPermissions],
        [ReportStatus.Paid, new[] { IrmRoles.CentralOffice }, ServiceCompanySapId, PaidAdminExpectedPermissions]
    ];

    [Theory]
    [MemberData(nameof(ScopedPermissionsTestData))]
    public void GetReportPermissions_ContainsExpected(ReportStatus reportStatus, IrmRoles[] actualRoles, string? serviceCompanySapId, string[] expected)
    {
        // arrange
        _userContextMock
            .Setup(x => x.GetRoles())
            .Returns(actualRoles);

        _userContextMock
            .Setup(x => x.IsInRole(IrmRoles.ServiceCompany))
            .Returns(actualRoles.Contains(IrmRoles.ServiceCompany));

        _userContextMock
            .Setup(x => x.ServiceCompanySapId)
            .Returns(ServiceCompanySapId);

        var report = new Report
        {
            ServiceCompanySapId = serviceCompanySapId ?? string.Empty,
            Number = "IRM00001/2026",
            ServiceDateFrom = default,
            ServiceDateTo = default,
            ServiceCompanyName = "sc-name",
            CreatedAt = default,
            CreatedBy = "user",
            UpdatedAt = default,
            UpdatedBy = "user"
        };

        var reportItem = new ReportItem
        {
            CouponNumber = "000011113333",
            ServiceName = "Название услуги",
            ServiceCompanySapId = "CompanySapId",
            ServiceCenterExternalId = "0022",
            ServiceCenterName = "Имя СЦ",
            CityKisId = "02383",
            CityName = "Урюпинск",
            ShopName = "S002",
            SearchText = "SearchText",
            UpdatedAt = DateTime.UtcNow,
            WareCode = "001",
            ServiceDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            BsiStatus = BsiStatus.Complete,
            AdditionalServices = []
        };

        reportItem.ApplyPrices(new Dictionary<(string WareCode, string Shop, DateOnly Date), PriceInfo>
        {
            [(reportItem.WareCode, reportItem.ShopName, reportItem.ServiceDate)] = new()
            {
                WareCode = reportItem.WareCode,
                ServiceDate = reportItem.ServiceDate,
                ShopName = reportItem.ShopName,
                Price = 234,
                ServiceCompanySapId = reportItem.ServiceCompanySapId,
                Nds = "UN"
            }
        });

        report.AddItems([reportItem]);

        if (reportStatus == ReportStatus.SentToPayment)
            report.MarkAsSendToPayment(Guid.NewGuid());

        if (reportStatus == ReportStatus.Paid)
        {
            report.MarkAsSendToPayment(Guid.NewGuid());
            report.MarkAsPaid();
        }

        // act
        var result = _service.GetReportPermissions(report);

        // assert
        result.Order().ShouldBe(expected.ToHashSet().Order());
    }

    private static readonly string[] AdminGlobalPermissions =
    [
        PermissionTypes.ReportReadAll,
        PermissionTypes.ReportExportRegistry,
        PermissionTypes.ServiceCompanyReadAll,
        PermissionTypes.MvzWrite,
        PermissionTypes.MvzReadAll
    ];

    private static readonly string[] ScGlobalPermissions =
    [
        PermissionTypes.ReportCreate,
        PermissionTypes.ReportExportRegistry
    ];

    public static IEnumerable<object[]> GlobalPermissionsTestData =>
    [
        [new[] { IrmRoles.Administrator }, AdminGlobalPermissions],
        [new[] { IrmRoles.ServiceCompany }, ScGlobalPermissions]
    ];

    [Theory]
    [MemberData(nameof(GlobalPermissionsTestData))]
    public void GetGlobalPermissions_ContainsExpected(IrmRoles[] actualRoles, string[] expected)
    {
        _userContextMock
            .Setup(x => x.GetRoles())
            .Returns(actualRoles);

        // act
        var result = _service.GetGlobalPermissions();

        // assert
        result.Order().ShouldBe(expected.ToHashSet().Order());
    }
}
