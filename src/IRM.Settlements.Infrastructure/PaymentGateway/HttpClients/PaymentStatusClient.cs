using System.Net.Http.Json;
using IRM.Settlements.Infrastructure.PaymentGateway.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace IRM.Settlements.Infrastructure.PaymentGateway.HttpClients;

public class PaymentStatusClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "payment_status_dictionary";
    private const int CacheDurationHours = 3;

    public PaymentStatusClient(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<IReadOnlyList<GetPaymentResponse>> GetPaymentsArrayAsync(List<GetPaymentRequest> request, CancellationToken ct)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/v1/payment/array", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<GetPaymentResponse>>(ct) ?? [];
    }

    public async Task<IReadOnlyList<PaymentStatusResponse>> GetDictionaryAsync(CancellationToken ct)
    {
        var result = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheDurationHours);

            using var response = await _httpClient.GetAsync("/api/v1/dictionary", ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<PaymentStatusResponse>>(ct) ?? [];
        });

        return result ?? [];
    }
}
