using System.Net.Http.Json;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class ReportSendToPaymentTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task SendToPayment_Success()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        // Arrange
        var report = await CreateDefaultReport();

        // Act
        var response = await HttpClient.PostAsync($"reports/{report.Id}/send-to-payment", null);
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await response.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();
        reportResult.Status.ShouldBe(ReportStatus.SentToPayment);

        var reportRepository = WebApplicationFactory.Services.GetRequiredService<IReportRepository>();
        var dbReport = await reportRepository.GetByIdAsync(report.Id);
        dbReport?.PaymentId.ShouldNotBeNull();
        dbReport?.SentToPaymentDate.ShouldNotBeNull();
        dbReport?.SentToPaymentDate!.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact(Skip = "Нужно выставить FeatureFlag UsePaymentGateway в false")]
    public async Task SendToPayment_IgnorePaymentGateway_Success()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        // Arrange
        var report = await CreateDefaultReport();

        // Act
        var response = await HttpClient.PostAsync($"reports/{report.Id}/send-to-payment", null);
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await response.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();
        reportResult.Status.ShouldBe(ReportStatus.SentToPayment);

        var reportRepository = WebApplicationFactory.Services.GetRequiredService<IReportRepository>();
        var dbReport = await reportRepository.GetByIdAsync(report.Id);
        dbReport?.PaymentId.ShouldBeNull();
        dbReport?.SentToPaymentDate.ShouldNotBeNull();
        dbReport?.SentToPaymentDate!.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }
}
