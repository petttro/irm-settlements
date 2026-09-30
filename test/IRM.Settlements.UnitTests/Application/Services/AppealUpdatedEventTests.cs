using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Integrations.IRM;
using IRM.Settlements.Domain.Enums;
using Moq;

namespace IRM.Settlements.UnitTests.Application.Services;

public class AppealUpdatedEventTests
{
    private readonly Mock<IAppealService> _appealServiceMock = new();

    [Theory]
    [InlineData(BsiStatus.New, PaymentStatus.Empty, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.InProgress, PaymentStatus.Empty, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.Cancel, PaymentStatus.Empty, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.TakenFromClient, PaymentStatus.Empty, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.Complete, PaymentStatus.AddedToTheReport, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.Complete, PaymentStatus.ReportOnTheRevision, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.Complete, PaymentStatus.Paid, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.Complete, PaymentStatus.Empty, ProjectType.DigitalAssistant)]
    [InlineData(BsiStatus.Complete, PaymentStatus.Empty, ProjectType.DigitalAssistantInStore)]
    public async Task Handle_ShouldNotProcessAppeal(BsiStatus bsiStatus, PaymentStatus paymentStatus, ProjectType projectType)
    {
        // Arrange
        var kafkaModel = new AppealUpdatedSnapshot
        {
            CouponNumber = "any",
            ServiceName = "any",
            CityName = "any",
            CityKisId = "any",
            ServiceCompanyName = "any",
            EnterpriseId = "any",
            ResolverExternalId = "any",
            WareCode = "any",
            ShopName = "any",
            BsiStatus = (int)bsiStatus,
            PaymentStatus = (int)paymentStatus,
            ProjectTypeId = (int)projectType,
            ResolverNameResolver = "any",
            SaleOrderNumber = "any",
            ServiceCompanyId = 18
        };

        var appealEvent = new AppealUpdatedMessage
        {
            Snapshot = kafkaModel,
            CreatedAt =  DateTime.UtcNow
        };

        var command = new AppealUpdatedEvent(appealEvent);

        // Act
        await AppealUpdatedEvent.AppealUpdatedEventHandler.Handle(command, _appealServiceMock.Object, CancellationToken.None);

        // Assert
        _appealServiceMock.Verify(r => r.AddOrUpdateAsync(It.IsAny<AppealUpdatedEvent>()), Times.Never);
    }

    [Theory]
    [InlineData(BsiStatus.Complete, PaymentStatus.Empty, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.MaterialDefect, PaymentStatus.Empty, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.MaterialDefectAfterInstallation, PaymentStatus.Empty, ProjectType.EquipmentInstallation)]
    [InlineData(BsiStatus.Complete, PaymentStatus.Inactive, ProjectType.EquipmentInstallation)]
    public async Task Handle_ShouldProcessAppeal(BsiStatus bsiStatus, PaymentStatus paymentStatus, ProjectType projectType)
    {
        var kafkaModel = new AppealUpdatedSnapshot
        {
            CouponNumber = "any",
            ServiceName = "any",
            CityName = "any",
            CityKisId = "any",
            ServiceCompanyName = "any",
            EnterpriseId = "any",
            ResolverExternalId = "any",
            WareCode = "any",
            ShopName = "any",
            BsiStatus = (int)bsiStatus,
            PaymentStatus = (int)paymentStatus,
            ProjectTypeId = (int)projectType,
            ResolverNameResolver = "any",
            SaleOrderNumber = "any",
            ServiceDate = DateTime.UtcNow,
            ServiceCompanyId = 18,
        };

        var appealEvent = new AppealUpdatedMessage
        {
            Snapshot = kafkaModel,
            CreatedAt =  DateTime.UtcNow
        };

        var command = new AppealUpdatedEvent(appealEvent);

        await AppealUpdatedEvent.AppealUpdatedEventHandler.Handle(command, _appealServiceMock.Object, CancellationToken.None);

        _appealServiceMock.Verify(r => r.AddOrUpdateAsync(It.IsAny<AppealUpdatedEvent>()), Times.Once);
    }
}
