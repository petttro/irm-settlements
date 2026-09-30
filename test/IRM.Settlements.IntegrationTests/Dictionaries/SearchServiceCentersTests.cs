using System.Net.Http.Json;
using IRM.Settlements.Api.Contracts.Dictionaries;
using IRM.Settlements.Application.UseCases.ServiceCenters;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.IntegrationTests.Fixtures;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Dictionaries;

public class SearchServiceCentersTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : BaseIntegrationTests(factory, testOutput)
{
    [Theory]
    [InlineData(IrmRoles.ServiceCompany)]
    [InlineData(IrmRoles.Administrator)]
    public async Task Should_Get_ServiceCenters_Success(IrmRoles role)
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

        var filter = $"{nameof(GetServiceCentersRequest.Search)}=гра&" +
                     $"{nameof(GetServiceCentersRequest.ServiceCompanySapId)}={ServiceCompanyId}";

        // ACT
        var response = await HttpClient.GetAsync($"service-centers?{filter}");
        await LogResponseOnFailureAsync(response);


        // ASSERT
        response.IsSuccessStatusCode.ShouldBeTrue();
        var responseContent = await response.Content.ReadFromJsonAsync<GetServiceCentersResult>();
        responseContent.ShouldNotBeNull();
        responseContent.Data.Count.ShouldBe(testData!.ServiceCenters.Count);

        for (var i = 0; i < responseContent.Data.Count; i++)
        {
            var actual = responseContent.Data[i];
            var expected = testData.ServiceCenters[i];

            actual.Name.ShouldBe(expected.Name);
            actual.ExternalId.ShouldBe(expected.ExternalId);
        }

    }
}
