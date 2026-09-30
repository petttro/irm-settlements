using System.Net;
using System.Net.Http.Json;
using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class GetReportsListTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task GetReports_Success()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        var reports = await CreateDefaultReports();
        var pageSize = 2;

        // Act
        var response = await HttpClient.GetAsync($"reports?" +
                                                   $"serviceCompanyId={ServiceCompanyId}" +
                                                   $"&pageIndex=0" +
                                                   $"&pageSize={pageSize}" +
                                                   $"&sort=-createdAt");

        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var reportsResult = await response.Content.ReadFromJsonAsync<GridResult<ReportResult>>();

        reportsResult.ShouldNotBeNull();
        reportsResult.TotalSize.ShouldBe(reports.Count);
        reportsResult.Data.Count.ShouldBe(pageSize);
        reportsResult.Data[0].CreatedAt.ShouldBeGreaterThan(reportsResult.Data[1].CreatedAt);
    }

    [Fact]
    public async Task GetReports_Search_Success()
    {
        // Arrange
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        await CreateDefaultReports();

        var searchText = testData!.Appeals[0].CouponNumber[1..];

        // Act
        var response = await HttpClient.GetAsync($"reports?search={searchText}&serviceCompanyId={ServiceCompanyId}");

        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var reportsResult = await response.Content.ReadFromJsonAsync<GridResult<ReportResult>>();

        reportsResult.ShouldNotBeNull();
        reportsResult.Data.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetReports_InvalidStatusFilter_ReturnsBadRequest()
    {
        // Arrange
        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.Administrator);

        // Act
        var response = await HttpClient.GetAsync("reports?status=wrong");

        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("Status");
    }

    [Fact]
    public async Task GetReports_DuplicateNotAllowedSort_Success()
    {
        // Arrange
        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.Administrator);

        // Act
        var response = await HttpClient.GetAsync("reports?&sort=wrong&&sort=-createdAt&sort=createdAt&sort=");

        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
    }
}
