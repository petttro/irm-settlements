using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.ValueObjects;
using Shouldly;

namespace IRM.Settlements.UnitTests.Domain.Entities;

public class ReportItemTests
{
    private ReportItem Create()
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
            WareCode = "0238473",
            SearchText = "SearchText",
            UpdatedAt = DateTime.UtcNow,
            ServiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };
    }

    [Theory]
    [InlineData(BsiStatus.Complete, 1000)]
    [InlineData(BsiStatus.MaterialDefect, 50 + 250)]
    [InlineData(BsiStatus.MaterialDefectAfterInstallation, 1000 + 50 + 250)]
    public void GetTotalCost_Depending_On_BsiStatus(BsiStatus bsiStatus, decimal expectedTotalCost)
    {
        // Arrange
        var reportItem = Create();
        reportItem.AdditionalServices =
        [
            new AdditionalService
            {
                Name = "Выезд",
                WareCode = "123",
            },
            new AdditionalService
            {
                Name = "Доставка",
                WareCode = "234",
            }
        ];

        var prices = new Dictionary<(string, string, DateOnly), PriceInfo>()
        {
            {
                new ValueTuple<string, string, DateOnly>(reportItem.WareCode, reportItem.ShopName, reportItem.ServiceDate),
                new PriceInfo
                {
                    ServiceCompanySapId = reportItem.ServiceCompanySapId,
                    WareCode = reportItem.WareCode,
                    ShopName = reportItem.ShopName,
                    ServiceDate = reportItem.ServiceDate,
                    Price = 1000,
                    Nds = "UN"
                }
            },
            {
                new ValueTuple<string, string, DateOnly>(reportItem.AdditionalServices[0].WareCode, reportItem.ShopName, reportItem.ServiceDate),
                new PriceInfo
                {
                    ServiceCompanySapId = reportItem.ServiceCompanySapId,
                    WareCode = reportItem.AdditionalServices[0].WareCode,
                    ShopName = reportItem.ShopName,
                    ServiceDate = reportItem.ServiceDate,
                    Price = 50,
                    Nds = "UN"
                }
            },
            {
                new ValueTuple<string, string, DateOnly>(reportItem.AdditionalServices[1].WareCode, reportItem.ShopName, reportItem.ServiceDate),
                new PriceInfo
                {
                    ServiceCompanySapId = reportItem.ServiceCompanySapId,
                    WareCode = reportItem.AdditionalServices[1].WareCode,
                    ShopName = reportItem.ShopName,
                    ServiceDate = reportItem.ServiceDate,
                    Price = 250,
                    Nds = "UN"
                }
            }
        };

        reportItem.BsiStatus = bsiStatus;
        reportItem.ApplyPrices(prices);

        // Act
        var actualTotalCost = reportItem.TotalCost;

        // Assert
        actualTotalCost.ShouldBe(expectedTotalCost);
    }
}
