using IRM.Settlements.Infrastructure.SAP.Exceptions;
using zmv_ws_get_prices_for_sp_bi;

namespace IRM.Settlements.Infrastructure.SAP.Clients;

public sealed class SapClient : ISapClient
{
    private readonly ZMV_WS_GET_PRICES_FOR_SP _client;

    public SapClient(ZMV_WS_GET_PRICES_FOR_SP client)
    {
        _client = client;
    }

    public async Task<ZMV_WS_GET_PRICES_FOR_SPResponse> GetPricesRawAsync(ZMV_WS_GET_PRICES_FOR_SPRequest request)
    {
        var transaction = SentrySdk.StartTransaction("sap.get_prices", "sap");
        var span = transaction.StartChild("sap.call", "soap");

        try
        {
            var itemsCount = request.ZMV_WS_GET_PRICES_FOR_SP.IT_INPUT?.Length ?? 0;
            span.SetTag("sap.service", "ZMV_WS_GET_PRICES_FOR_SP");
            span.SetTag("sap.operation", "GetPrices");
            span.SetTag("sap.items_count", itemsCount.ToString());

            var response = await _client.ZMV_WS_GET_PRICES_FOR_SPAsync(request);
            var result = response?.ZMV_WS_GET_PRICES_FOR_SPResponse ?? throw new SapInvalidResponseException("Sap Response is null");
            span.Finish(SpanStatus.Ok);

            return result;
        }
        catch (Exception ex)
        {
            span.Finish(ex);
            SentrySdk.CaptureException(ex);
            throw;
        }
        finally
        {
            transaction.Finish();
        }
    }
}
