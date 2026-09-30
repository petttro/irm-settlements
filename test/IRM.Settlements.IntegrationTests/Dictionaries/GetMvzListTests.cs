using System.Net;
using System.Net.Http.Json;
using IRM.Settlements.Api.Contracts.Dictionaries;
using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.UseCases.Mvz;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Dictionaries;

public class GetMvzListTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : BaseIntegrationTests(factory, testOutput)
{
    [Theory]
    [InlineData(IrmRoles.Administrator)]
    [InlineData(IrmRoles.SeniorManager)]
    [InlineData(IrmRoles.CentralOffice)]
    public async Task Get_MvzList_Success(IrmRoles role)
    {
        // ARRANGE
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);
        testData.ShouldNotBeNull();

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData);

        HttpClient
            .WithUser("345")
            .WithRoles(role);

        var filter = $"{nameof(GetMvzListRequest.Search)}=02&{nameof(GetServiceCompaniesRequest.PageSize)}=10";

        // ACT
        var response = await HttpClient.GetAsync($"mvz?{filter}");
        await LogResponseOnFailureAsync(response);

        // ASSERT
        response.IsSuccessStatusCode.ShouldBeTrue();
        var responseContent = await response.Content.ReadFromJsonAsync<GridResult<MvzItemResult>>();
        responseContent.ShouldNotBeNull();
        responseContent.Data.Count.ShouldBe(1);
        responseContent.PaginatedQuery.PageIndex.ShouldBe(0);
        responseContent.PaginatedQuery.PageSize.ShouldBe(10);
        responseContent.TotalSize.ShouldBe(1);

        var actual = responseContent.Data[0];
        var expected = testData.Mvz.Find(mvz => mvz.Id.Contains("02"));
        expected.ShouldNotBeNull();

        actual.ShopName.ShouldBe(expected.Id);
    }

    [Fact]
    public async Task GetMvzList_ServiceCompany_ReturnsForbidden()
    {
        // ARRANGE
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(IrmRoles.ServiceCompany);

        var filter = $"{nameof(GetMvzListRequest.Search)}=02&{nameof(GetServiceCompaniesRequest.PageSize)}=10";

        // ACT
        var response = await HttpClient.GetAsync($"mvz?{filter}");
        await LogResponseOnFailureAsync(response);

        // ASSERT
        response.IsSuccessStatusCode.ShouldBe(false);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var responseContent = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        responseContent.ShouldNotBeNull();
    }
}
