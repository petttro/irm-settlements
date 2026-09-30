using System.Net.Http.Json;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.IntegrationTests.Fixtures;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class ReportRecalculateCostTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task RecalculateCost_Success()
    {
        // Arrange
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);
        testData.ShouldNotBeNull();
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        var report = await CreateDefaultReport();

        // Act
        var response = await HttpClient.PostAsync($"reports/{report.Id}/recalculate-cost", null);
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await response.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();

        var expectedCost = testData.Appeals
            .Where(a => a.BsiStatus == BsiStatus.Complete || a.BsiStatus == BsiStatus.MaterialDefectAfterInstallation)
            .Sum(a => a.ServicePrice + FakeSapClient.SapPriceAddition)
                           // Тут добавляем 2 по 2 доп услуги для не Completed BsiStatus
                           + FakeSapClient.AdditionalServicePrice * 4;

        reportResult.TotalCost.ShouldBe(expectedCost);

        var reportItems = await GetReportItems(report.Id);
        foreach (var reportItem in reportItems.Data)
        {
            var appeal = testData.Appeals.Find(a => a.CouponNumber == reportItem.CouponNumber);
            appeal.ShouldNotBeNull();

            if (appeal.BsiStatus == BsiStatus.Complete)
            {
                reportItem.Cost.ShouldBe(appeal.ServicePrice+ FakeSapClient.SapPriceAddition);
                foreach (var additionalService in reportItem.AdditionalServices)
                {
                    additionalService.Cost.ShouldBe(0);
                }
            }

            if (appeal.BsiStatus == BsiStatus.MaterialDefect)
            {
                reportItem.Cost.ShouldBe(0);
                foreach (var additionalService in reportItem.AdditionalServices)
                {
                    additionalService.Cost.ShouldBe(FakeSapClient.AdditionalServicePrice);
                }
            }

            if (appeal.BsiStatus == BsiStatus.MaterialDefectAfterInstallation)
            {
                reportItem.Cost.ShouldBe(appeal.ServicePrice + FakeSapClient.SapPriceAddition);
                foreach (var additionalService in reportItem.AdditionalServices)
                {
                    additionalService.Cost.ShouldBe(FakeSapClient.AdditionalServicePrice);
                }
            }
        }
    }
}
