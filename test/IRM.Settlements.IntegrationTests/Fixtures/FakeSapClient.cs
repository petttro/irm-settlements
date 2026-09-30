using IRM.Settlements.Infrastructure.SAP.Clients;
using Newtonsoft.Json;
using zmv_ws_get_prices_for_sp_bi;

namespace IRM.Settlements.IntegrationTests.Fixtures;

public class FakeSapClient : ISapClient
{
    public const decimal SapPriceAddition = 20;
    public const decimal AdditionalServicePrice = 222;

    public async Task<ZMV_WS_GET_PRICES_FOR_SPResponse> GetPricesRawAsync(ZMV_WS_GET_PRICES_FOR_SPRequest request)
    {
        var appealsJson = await LoadFile("SettlementsTestData.json");
        var testData = JsonConvert.DeserializeObject<TestDataRoot>(appealsJson);

        if (testData == null)
            throw new Exception("Could not load SettlementsTestData.json");

        var output = testData.Appeals
            .SelectMany(a =>
            {
                var serviceCompanySapId = testData.ServiceCompanies
                    .Find(s => s.Id == a.ServiceCompanyId)!.SapId;

                // основная услуга
                var baseItem = new[]
                {
                    new ZMVS_GET_PRICES_FOR_SP_OUTPUT
                    {
                        CN_NDS = "UN",
                        // для тестов чтобы различать
                        KBETR = a.ServicePrice + SapPriceAddition,
                        LIFNR = serviceCompanySapId,
                        MATNR = a.WareCode.PadLeft(18, '0'),
                        WERKS = a.ShopName,
                        ZDATE = a.ServiceDate.ToString("yyyy-MM-dd")
                    }
                };

                // дополнительные услуги
                var additionalItems = a.AdditionalServices
                    .Select(s => new ZMVS_GET_PRICES_FOR_SP_OUTPUT
                    {
                        CN_NDS = "UN",
                        // можно варьировать цену для тестов
                        KBETR = AdditionalServicePrice,
                        LIFNR = serviceCompanySapId,
                        MATNR = s.WareCode.PadLeft(18, '0'),
                        WERKS = a.ShopName,
                        ZDATE = a.ServiceDate.ToString("yyyy-MM-dd")
                    });

                return baseItem.Concat(additionalItems);
            })
            .ToArray();

        return new ZMV_WS_GET_PRICES_FOR_SPResponse { ET_OUTPUT = output };
    }

    private static async Task<string> LoadFile(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", name);
        return await File.ReadAllTextAsync(path);
    }
}
