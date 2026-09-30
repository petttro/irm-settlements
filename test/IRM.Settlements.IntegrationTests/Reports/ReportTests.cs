using System.Net.Http.Json;
using IRM.Settlements.Api.Contracts.Reports;
using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class ReportTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : BaseIntegrationTests(factory, testOutput)
{
    protected async Task<List<ReportResult>> CreateDefaultReports()
    {
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        var baseRequest = new CreateReportRequest
        {
            ServiceDateFrom = DateOnly.Parse("2026-01-01"),
            ServiceDateTo = DateOnly.Parse("2026-12-01"),
            ServiceCompanySapId = ServiceCompanyId,
        };

        HttpClient
            .WithUser("TestUser")
            .WithServiceCompanyId(baseRequest.ServiceCompanySapId)
            .WithRoles(IrmRoles.ServiceCompany);

        var results = new List<ReportResult>();

        foreach (var appeal in testData!.Appeals)
        {
            var request = baseRequest with
            {
                ReportName = "Отчет " + appeal.CouponNumber,
                ServiceDateFrom = appeal.ServiceDate,
                ServiceDateTo = appeal.ServiceDate,
            };

            var createResponse = await HttpClient.PostAsJsonAsync("reports", request);
            await LogResponseOnFailureAsync(createResponse);

            var result = await createResponse.Content.ReadFromJsonAsync<ReportResult>();
            results.Add(result!);
        }

        return results;
    }

    protected async Task<ReportResult> CreateDefaultReport(CreateReportRequest? request = null)
    {
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        if (request == null)
        {
            request = new CreateReportRequest
            {
                ReportName = "Test Report",
                ServiceDateFrom = DateOnly.Parse("2026-01-01"),
                ServiceDateTo = DateOnly.Parse("2026-12-01"),
                ServiceCompanySapId = ServiceCompanyId,
            };
        }

        HttpClient
            .WithUser("TestUser")
            .WithServiceCompanyId(request.ServiceCompanySapId!)
            .WithRoles(IrmRoles.ServiceCompany);

        var createResponse = await HttpClient.PostAsJsonAsync("reports", request);
        await LogResponseOnFailureAsync(createResponse);

        var result = await createResponse.Content.ReadFromJsonAsync<ReportResult>();
        return result!;
    }

    protected async Task<ReportResult> ReportSendToPayment(Guid reportId)
    {
        var response = await HttpClient.PostAsync($"reports/{reportId}/send-to-payment", null);
        await LogResponseOnFailureAsync(response);
        var result = await response.Content.ReadFromJsonAsync<ReportResult>();

        return result!;
    }

    protected async Task<ReportItemsResult> GetReportItems(Guid reportId)
    {
        var reportItemsResponse = await HttpClient.GetAsync($"reports/{reportId}/items");
        await LogResponseOnFailureAsync(reportItemsResponse);
        var reportItemsResult = await reportItemsResponse.Content.ReadFromJsonAsync<ReportItemsResult>();

        return reportItemsResult!;
    }

    protected async Task<ReportResult> GetReport(Guid reportId)
    {
        var reportItemsResponse = await HttpClient.GetAsync($"reports/{reportId}");
        await LogResponseOnFailureAsync(reportItemsResponse);
        var reportItemsResult = await reportItemsResponse.Content.ReadFromJsonAsync<ReportResult>();

        return reportItemsResult!;
    }

    protected async Task<Report?> GetReportFromDb(Guid reportId)
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();

        return await reportRepository.GetByIdAsync(reportId);
    }

    protected async Task SaveReportsToDb(List<Report> reports)
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SettlementsDbContext>();

        await dbContext.AddRangeAsync(reports);
        await dbContext.SaveChangesAsync();
    }
}
