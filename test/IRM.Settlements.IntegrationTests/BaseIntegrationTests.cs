using IRM.Settlements.Application.Abstractions.Repositories;
using IRM.Settlements.Application.Abstractions.Services;
using IRM.Settlements.Application.Integrations.IRM;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using IRM.Settlements.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace IRM.Settlements.IntegrationTests;

[Collection(IntegrationTestsCollection.Name)]
public class BaseIntegrationTests
{
    protected readonly string ServiceCompanyId = "K000012097";

    protected BaseIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper testOutput)
    {
        WebApplicationFactory = factory;
        TestOutput = testOutput;
        HttpClient = factory.CreateClient();
        HttpClient.BaseAddress = new Uri("http://localhost/api/");
    }

    protected HttpClient HttpClient { get; }

    protected ITestOutputHelper TestOutput { get; }

    protected TestWebApplicationFactory WebApplicationFactory { get; }

    protected async Task LogResponseOnFailureAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            TestOutput.WriteLine("METHOD: " + response.RequestMessage?.Method);
            TestOutput.WriteLine("URL: " + response.RequestMessage?.RequestUri);
            TestOutput.WriteLine("STATUS: " + response.StatusCode);
            TestOutput.WriteLine("==============================================================");
            TestOutput.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }

    protected static async Task Eventually(Func<Task> assertion, int timeoutMs = 5000)
    {
        var start = DateTime.UtcNow;

        while (true)
        {
            try
            {
                await assertion();
                return;
            }
            catch
            {
                if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMs)
                    throw;

                await Task.Delay(200);
            }
        }
    }

    protected static async Task<string> LoadFile(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", name);
        return await File.ReadAllTextAsync(path);
    }

    protected async Task<Appeal?> GetAppealsFromDb(int appealId)
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var appealRepository = scope.ServiceProvider.GetRequiredService<IAppealRepository>();

        var addedAppeals = await appealRepository.GetByIdAsync(appealId, CancellationToken.None);

        return addedAppeals;
    }

    protected async Task AddAppealToDb(AppealUpdatedEvent appeal)
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var appealService = scope.ServiceProvider.GetRequiredService<IAppealService>();

        await appealService.AddOrUpdateAsync(appeal);
    }

    protected async Task<List<Appeal>> GetAllAppealsAsync()
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var appealRepository = scope.ServiceProvider.GetRequiredService<IAppealRepository>();

        var appeals = await appealRepository.GetAllAsync();

        return appeals.ToList();
    }

    protected async Task InitializeTestData(TestDataRoot testData)
    {
        using var scope = WebApplicationFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SettlementsDbContext>();

        db.ServiceCompanies.AddRange(testData.ServiceCompanies);
        db.ServiceCenters.AddRange(testData.ServiceCenters);
        db.Mvz.AddRange(testData.Mvz);
        db.Appeals.AddRange(testData.Appeals);

        await db.SaveChangesAsync();
    }
}
