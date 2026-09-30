using System.Net.Http.Json;

namespace IRM.Settlements.Infrastructure.IrmCatalog.HttpClients;

public class IrmCatalogHttpClient
{
    private readonly HttpClient _httpClient;

    public IrmCatalogHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceCompanyCatalogResponse?> GetServiceCompanyAsync(string serviceCompanySapId, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync($"/api/v1/ServiceCompanies/{serviceCompanySapId}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ServiceCompanyCatalogResponse>(ct);
    }

    public async Task<ServiceCompanyCatalogQueryResponse?> QueryServiceCompaniesAsync(ServiceCompanyQueryRequest request, CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/ServiceCompanies/query", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ServiceCompanyCatalogQueryResponse>(ct);
    }
}
