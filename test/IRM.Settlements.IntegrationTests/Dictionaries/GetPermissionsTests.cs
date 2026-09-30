using System.Net.Http.Json;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.Domain.Permissions;
using IRM.Settlements.IntegrationTests.Fixtures;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Dictionaries;

public class GetPermissionsTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : BaseIntegrationTests(factory, testOutput)
{
    [Fact]
    public async Task ServiceCompany_CanReadAndWrite_Success()
    {
        // ARRANGE
        var serviceCompanyId = "K000007490";

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(serviceCompanyId)
            .WithRoles(IrmRoles.ServiceCompany);

        // ACT
        var response = await HttpClient.GetAsync("permissions");
        await LogResponseOnFailureAsync(response);

        // ASSERT
        response.IsSuccessStatusCode.ShouldBeTrue();
        var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();

        responseContent.ShouldNotBeNull();
        responseContent.ShouldBeEquivalentTo(new List<string>
        {
            PermissionTypes.ReportCreate,
            PermissionTypes.ReportExportRegistry
        });
    }

    [Fact]
    public async Task Admin_CanReadAndNotWrite_Success()
    {
        // ARRANGE
        var serviceCompanyId = "K000007490";

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(serviceCompanyId)
            .WithRoles(IrmRoles.Administrator);

        // ACT
        var response = await HttpClient.GetAsync("permissions");
        await LogResponseOnFailureAsync(response);

        // ASSERT
        response.IsSuccessStatusCode.ShouldBeTrue();
        var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();

        responseContent.ShouldNotBeNull();
        responseContent.ShouldBeEquivalentTo(new List<string>
        {
            PermissionTypes.ReportReadAll,
            PermissionTypes.ReportExportRegistry,
            PermissionTypes.ServiceCompanyReadAll,
            PermissionTypes.MvzReadAll,
            PermissionTypes.MvzWrite
        });
    }

    [Fact]
    public async Task NotAuthenticated_DontHavePermissions_Success()
    {
        // ARRANGE
        // ACT
        var response = await HttpClient.GetAsync("permissions");
        await LogResponseOnFailureAsync(response);

        // ASSERT
        response.IsSuccessStatusCode.ShouldBeTrue();
        var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();

        var permissions = responseContent == null ? [] : responseContent.ToArray();

        permissions.ShouldNotBeNull();
        permissions.ShouldBeEmpty();
    }
}
