using System.Net;
using System.Net.Http.Json;
using IRM.Settlements.Application.UseCases.Reports.Commands;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Infrastructure.PaymentGateway.Contracts;
using IRM.Settlements.IntegrationTests.Fixtures;
using IRM.Settlements.IntegrationTests.Reports;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Wolverine;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Integrations;

public class GetPaymentStatusTests : ReportTests
{
    public GetPaymentStatusTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
        : base(factory, testOutput)
    {
    }

    [Fact]
    public async Task PaymentGateway_GetPaymentStatus_Job_Success()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        var report = await CreateDefaultReport();
        await ReportSendToPayment(report.Id);

        var savedReport = await GetReportFromDb(report.Id);
        savedReport.ShouldNotBeNull();
        savedReport.PaymentId.ShouldNotBeNull();

        var paymentGatewayResponse = new List<GetPaymentResponse>
        {
            new()
            {
                PaymentId = savedReport.PaymentId!.Value,
                LastStatusId = 1,
                MerchantId = 4
            }
        };

        WebApplicationFactory.CurrentHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(paymentGatewayResponse)
        };

        // Act
        var bus =  WebApplicationFactory.Services.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new CheckPaymentsCommand());

        // Assert
        await Eventually(async () =>
        {
            var paidReport = await GetReport(report.Id);
            paidReport.Status.ShouldBe(ReportStatus.Paid);
            paidReport.PaymentDate.ShouldNotBeNull();
            paidReport.SentToPaymentDate!.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        });
    }
}
