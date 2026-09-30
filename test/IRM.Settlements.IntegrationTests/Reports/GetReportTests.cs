using System.Net;
using System.Net.Http.Json;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Permissions;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Reports;

public class GetReportTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput) : ReportTests(factory, testOutput)
{
    [Fact]
    public async Task GetReport_Success()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        var report = await CreateDefaultReport();

        // Act
        var response = await HttpClient.GetAsync($"reports/{report.Id}");
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();

        var actualReport = await response.Content.ReadFromJsonAsync<ReportResult>();
        var expectedReport = report;

        actualReport.ShouldNotBeNull();

        actualReport.Name.ShouldBe("Test Report");
        actualReport.ServiceCompanySapId.ShouldBe(expectedReport.ServiceCompanySapId);
        actualReport.ServiceCompanyName.ShouldBe(expectedReport.ServiceCompanyName);
        actualReport.Id.ShouldBe(expectedReport.Id);
        actualReport.Status.ShouldBe(expectedReport.Status);
        actualReport.SentToPaymentDate.ShouldBe(expectedReport.SentToPaymentDate);
        actualReport.Status.ShouldBe(expectedReport.Status);
    }

    [Theory]
    [InlineData(IrmRoles.Administrator)]
    [InlineData(IrmRoles.SeniorManager)]
    [InlineData(IrmRoles.CentralOffice)]
    [InlineData(IrmRoles.ServiceCompany)]
    public async Task GetReport_RoleCanRead_Success(IrmRoles role)
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        var report = await CreateDefaultReport();
        HttpClient.WithRoles(role);

        // Act
        var response = await HttpClient.GetAsync($"reports/{report.Id}");
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var reportResult = await response.Content.ReadFromJsonAsync<ReportResult>();
        reportResult.ShouldNotBeNull();

        if (role is IrmRoles.ServiceCompany)
        {
            reportResult.Permissions.ShouldBeEquivalentTo(
                new List<string>
                {
                    PermissionTypes.ReportRead,
                    PermissionTypes.ReportWrite,
                    PermissionTypes.ReportDelete,
                    PermissionTypes.ReportRecalculate,
                    PermissionTypes.ReportSendToPayment,
                    PermissionTypes.ReportExport,
                    PermissionTypes.ServiceCenterRead
                });
        }
        else
        {
            reportResult.Permissions.ShouldBeEquivalentTo(new List<string>
            {
                PermissionTypes.ReportRead,
                PermissionTypes.ReportDelete,
                PermissionTypes.ReportRecalculate,
                PermissionTypes.ReportExport,
                PermissionTypes.ServiceCenterRead
            });
        }
    }

    [Fact]
    public async Task GetReport_UserDontHaveReadAccessToServiceCompany_ReturnsForbidden()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();

        var report = await CreateDefaultReport();

        var wrongServiceCompany = "Wrong service company";
        HttpClient.WithServiceCompanyId(wrongServiceCompany);

        // Act
        var response = await HttpClient.GetAsync($"reports/{report.Id}");
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Detail.ShouldBe($"Action {PermissionTypes.ReportRead} not allowed for {ServiceCompanyId} in Status {report.Status}");
    }

    [Fact]
    public async Task GetReport_WithApiKey_Success()
    {
        // Arrange
        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        var report = await CreateDefaultReport();
        HttpClient.DefaultRequestHeaders.Clear();
        HttpClient.DefaultRequestHeaders.Add("x-api-key", "test-api-key");

        // Act
        var response = await HttpClient.GetAsync($"reports/{report.Id}");
        await LogResponseOnFailureAsync(response);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
    }
}
