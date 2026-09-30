using System.Net.Http.Json;
using IRM.Settlements.Api.Contracts.Dictionaries;
using IRM.Settlements.Application.UseCases.ServiceCompanies;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.IntegrationTests.Fixtures;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Dictionaries;

public class SearchServiceCompaniesTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : BaseIntegrationTests(factory, testOutput)
{
    [Theory]
    [InlineData(IrmRoles.ServiceCompany)]
    [InlineData(IrmRoles.Administrator)]
    public async Task Should_Get_ServiceCompanies_Success(IrmRoles role)
    {
        // ARRANGE
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles(role);

        var filter = $"{nameof(GetServiceCompaniesRequest.Search)}=гра&{nameof(GetServiceCompaniesRequest.PageSize)}=10";

        // ACT
        var response = await HttpClient.GetAsync($"service-companies?{filter}");
        await LogResponseOnFailureAsync(response);

        // ASSERT
        response.IsSuccessStatusCode.ShouldBeTrue();
        var responseContent = await response.Content.ReadFromJsonAsync<GetServiceCompaniesResult>();
        responseContent.ShouldNotBeNull();
        responseContent.Data.Count.ShouldBe(testData!.ServiceCompanies.Count);

        var actual = responseContent.Data[0];
        var expected = testData.ServiceCompanies[0];

        actual.Name.ShouldBe(expected.Name);
        actual.SapId.ShouldBe(expected.SapId);
    }
}
