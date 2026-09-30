using System.Net;
using System.Net.Http.Json;
using IRM.Settlements.Api.Contracts.Dictionaries;
using IRM.Settlements.Application.UseCases.Mvz;
using IRM.Settlements.Domain.Enums;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests.Dictionaries;

public class UpdateMvzItemTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    : BaseIntegrationTests(factory, testOutput)
{
    [Theory]
    [InlineData(IrmRoles.Administrator)]
    [InlineData(IrmRoles.SeniorManager)]
    [InlineData(IrmRoles.CentralOffice)]
    public async Task Update_MvzItem_Success(IrmRoles role)
    {
        // ARRANGE
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        HttpClient
            .WithUser("345")
            .WithRoles(role);

        var shopName = "S002";
        var request = new UpdateMvzItemRequest { MvzCode = "aaa", MvzName = "bbb" };

        // ACT
        var response = await HttpClient.PutAsJsonAsync($"mvz/{shopName}", request);
        await LogResponseOnFailureAsync(response);

        // ASSERT
        response.IsSuccessStatusCode.ShouldBeTrue();
        var responseContent = await response.Content.ReadFromJsonAsync<MvzItemResult>();
        responseContent.ShouldNotBeNull();

        responseContent.ShopName.ShouldBe(shopName);
        responseContent.MvzCode.ShouldBe(request.MvzCode);
        responseContent.MvzName.ShouldBe(request.MvzName);
    }

    [Fact]
    public async Task Update_MvzItem_ServiceCompany_ReturnsForbidden()
    {
        // ARRANGE
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        await WebApplicationFactory.PostgresFixture.ResetDatabaseAsync();
        await InitializeTestData(testData!);

        HttpClient
            .WithUser("345")
            .WithServiceCompanyId(ServiceCompanyId)
            .WithRoles([IrmRoles.ServiceCompany]);

        var request = new UpdateMvzItemRequest { MvzCode = "aaa", MvzName = "bbb" };

        // ACT
        var response = await HttpClient.PutAsJsonAsync($"mvz/S002", request);
        await LogResponseOnFailureAsync(response);

        // ASSERT
        response.IsSuccessStatusCode.ShouldBe(false);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var responseContent = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        responseContent.ShouldNotBeNull();
    }
}
