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

public class ReportConfirmPaymentTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task ConfirmPayment_SentToPayment_To_Paid_Success()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        // Arrange
        var report = await CreateDefaultReport();

        var response1 = await HttpClient.PostAsync($"reports/{report.Id}/send-to-payment", null);
        await LogResponseOnFailureAsync(response1);

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.SeniorManager);

        // Act
        var response = await HttpClient.PostAsync($"reports/{report.Id}/confirm-payment", null);
        await LogResponseOnFailureAsync(response);


        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await response.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();
        reportResult.Status.ShouldBe(ReportStatus.Paid);

        var reportRepository = WebApplicationFactory.Services.GetRequiredService<IReportRepository>();
        var dbReport = await reportRepository.GetByIdAsync(report.Id);
        dbReport?.PaymentId.ShouldNotBeNull();
        dbReport?.PaymentDate.ShouldNotBeNull();
        dbReport?.PaymentDate!.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task ConfirmPayment_Paid_To_ConfirmPayment_Success()
    {
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        // Arrange
        var report = await CreateDefaultReport();

        var response1 = await HttpClient.PostAsync($"reports/{report.Id}/send-to-payment", null);
        await LogResponseOnFailureAsync(response1);

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.SeniorManager);

        var response2 = await HttpClient.PostAsync($"reports/{report.Id}/confirm-payment", null);
        await LogResponseOnFailureAsync(response2);

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
        dbReport?.SentToPaymentDate.ShouldNotBeNull();
        dbReport?.SentToPaymentDate!.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }
}
