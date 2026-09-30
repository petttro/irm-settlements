using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.ValueObjects;
using Shouldly;


namespace IRM.Settlements.UnitTests.Domain.Entities;

public class ReportTests
{
    private Report CreateReport()
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            Number = "IRM00001/2026",
            Name = "test",
            ServiceDateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            ServiceDateTo = DateOnly.FromDateTime(DateTime.UtcNow),
            ServiceCompanySapId = "sap",
            ServiceCompanyName = "name",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "user"
        };
    }

    private ReportItem CreateItem()
    {
        return new ReportItem
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
            AdditionalServices =
            [
                new AdditionalService
                {
                    Name = "Name 1",
                    WareCode = "0011"
                },
                new AdditionalService
                {
                    Name = "Name 2",
                    WareCode = "0012"
                }
            ]
        };
    }

    [Fact]
    public void ApplyPrices_Should_Set_Total_And_Count()
    {
        // Arrange
        var report = CreateReport();

        var item1 = CreateItem();
        item1.BsiStatus = BsiStatus.Complete;

        var item2 = CreateItem();
        item2.BsiStatus = BsiStatus.MaterialDefect;

        var item3 = CreateItem();
        item3.BsiStatus = BsiStatus.MaterialDefectAfterInstallation;

        report.AddItems([item1, item2, item3]);

        const decimal mainPrice = 123;
        const decimal additionalPrice1 = 234;
        const decimal additionalPrice2 = 456;

        List<PriceInfo> prices =
        [
            new()
            {
                ServiceCompanySapId = report.ServiceCompanySapId,
                WareCode = item1.WareCode,
                ShopName = item1.ShopName,
                ServiceDate = item1.ServiceDate,
                Price = mainPrice,
                Nds = "UN"
            },
            new()
            {
                ServiceCompanySapId = report.ServiceCompanySapId,
                WareCode = item1.AdditionalServices[0].WareCode,
                ShopName = item1.ShopName,
                ServiceDate = item1.ServiceDate,
                Price = additionalPrice1,
                Nds = "UN"
            },
            new()
            {
                ServiceCompanySapId = report.ServiceCompanySapId,
                WareCode = item1.AdditionalServices[1].WareCode,
                ShopName = item1.ShopName,
                ServiceDate = item1.ServiceDate,
                Price = additionalPrice2,
                Nds = "UN"
            }
        ];

        // Act
        report.ApplyPrices(prices);

        // Assert
        report.ItemsCount.ShouldBe(3);
        report.TotalCost.ShouldBe(mainPrice +
                                  mainPrice + additionalPrice1 + additionalPrice2 +
                                  additionalPrice1 + additionalPrice2);
    }

    [Fact]
    public void DeleteItems_All_Should_Throw_Exception()
    {
        // Arrange
        var report = CreateReport();
        var item1 = CreateItem();
        item1.CouponNumber = "01";
        var item2 = CreateItem();
        item2.CouponNumber = "02";
        report.AddItems([item1, item2]);

        // Act & Assert
        var exception = Should.Throw<ReportIncorrectStateException>(() => report.RemoveItems([item1.CouponNumber, item2.CouponNumber]));
        exception.Message.ShouldBe("Отчёт не может быть пустым. Должен содержать хотя бы один элемент.");
    }

    [Fact]
    public void SendToPayment_Null_MainCost_Should_Throw_Exception()
    {
        // Arrange
        var report = CreateReport();

        // нулевая сумма основной цены
        var item1 = CreateItem();
        item1.CouponNumber = "01";
        item1.BsiStatus = BsiStatus.Complete;
        item1.AdditionalServices = [];

        // нулевая сумма доп услуг
        var item2 = CreateItem();
        item2.CouponNumber = "02";
        item2.BsiStatus = BsiStatus.MaterialDefect;
        item2.AdditionalServices = [];

        // корректная цена завершенной услуги
        var item3 = CreateItem();
        item3.CouponNumber = "03";
        item3.WareCode = "Item3WareCode";
        item3.BsiStatus = BsiStatus.Complete;
        item3.AdditionalServices = [];

        // корректная цена доп услуги с дефектами
        var item4 = CreateItem();
        item4.CouponNumber = "04";
        item4.BsiStatus = BsiStatus.MaterialDefect;
        item4.AdditionalServices[1].WareCode = "Item4WareCode";

        const decimal mainPrice = 123;
        const decimal additionalPrice1 = 234;

        List<PriceInfo> prices =
        [
            new()
            {
                ServiceCompanySapId = report.ServiceCompanySapId,
                WareCode = item3.WareCode,
                ShopName = item3.ShopName,
                ServiceDate = item3.ServiceDate,
                Price = mainPrice,
                Nds = "UN"
            },
            new()
            {
                ServiceCompanySapId = report.ServiceCompanySapId,
                WareCode = item4.AdditionalServices[0].WareCode,
                ShopName = item4.ShopName,
                ServiceDate = item4.ServiceDate,
                Price = additionalPrice1,
                Nds = "UN"
            }
        ];

        report.AddItems([item1, item2, item3, item4]);
        report.ApplyPrices(prices);

        // Act & Assert
        var exception = Should.Throw<ReportIncorrectStateException>(() => report.MarkAsSendToPayment());
        exception.Message.ShouldBe("Отчет не может быть отправлен на оплату. Найдены услуги с нулевой ценой: 01, 02 . " +
                                   "Вы можете: удалить услуги из отчета и отправить на оплату или обратиться в центральный офис. " +
                                   "После внесения цен на услуги со стороны офиса, отчет может быть передан в оплату.");
    }
}
