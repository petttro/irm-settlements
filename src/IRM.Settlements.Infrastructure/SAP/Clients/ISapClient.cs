using zmv_ws_get_prices_for_sp_bi;

namespace IRM.Settlements.Infrastructure.SAP.Clients;

public interface ISapClient
{
    Task<ZMV_WS_GET_PRICES_FOR_SPResponse> GetPricesRawAsync(ZMV_WS_GET_PRICES_FOR_SPRequest request);
}
