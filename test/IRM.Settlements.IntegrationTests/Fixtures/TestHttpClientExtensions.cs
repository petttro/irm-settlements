using IRM.Settlements.Domain.Enums;

namespace IRM.Settlements.IntegrationTests.Fixtures;

public static class HttpClientExtensions
{
    public static HttpClient WithUser(this HttpClient client, string userId)
    {
        client.DefaultRequestHeaders.Remove("x-test-user-id");
        client.DefaultRequestHeaders.Add("x-test-user-id", userId);

        return client;
    }

    public static HttpClient WithRoles(this HttpClient client, params IrmRoles[] roles)
    {
        client.DefaultRequestHeaders.Remove("x-test-roles");
        client.DefaultRequestHeaders.Add("x-test-roles", string.Join(",", roles));

        return client;
    }

    public static HttpClient WithServiceCompanyId(this HttpClient client, string serviceCompanyId)
    {
        client.DefaultRequestHeaders.Remove("x-test-service-company-id");
        client.DefaultRequestHeaders.Add("x-test-service-company-id", serviceCompanyId);

        return client;
    }

    public static HttpClient WithApiKey(this HttpClient client, string apiKey)
    {
        client.DefaultRequestHeaders.Remove("x-api-key");
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);

        return client;
    }
}
