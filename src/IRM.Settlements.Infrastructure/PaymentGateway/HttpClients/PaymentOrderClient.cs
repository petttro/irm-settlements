using System.Net.Http.Json;
using IRM.Settlements.Infrastructure.PaymentGateway.Contracts;

namespace IRM.Settlements.Infrastructure.PaymentGateway.HttpClients;

public class PaymentOrderClient
{
    private readonly HttpClient _httpClient;

    public PaymentOrderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendPaymentsAsync(List<SendPaymentRequest> payments, CancellationToken ct)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/v1/payment", payments, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }
}
