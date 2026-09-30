using System.Globalization;
using IRM.Settlements.Application.Abstractions.SAP;
using IRM.Settlements.Application.Integrations.SAP;
using IRM.Settlements.Infrastructure.SAP.Clients;
using IRM.Settlements.Infrastructure.SAP.Exceptions;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using zmv_ws_get_prices_for_sp_bi;

namespace IRM.Settlements.Infrastructure.SAP;

public sealed class SapIntegrationService : ISapIntegrationService
{
    private const int BatchSize = 500;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly ISapClient _client;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly ILogger<SapIntegrationService> _logger;

    public SapIntegrationService(
        ISapClient client,
        ILogger<SapIntegrationService> logger,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _client = client;
        _logger = logger;
        _pipelineProvider = pipelineProvider;
    }

    public async Task<IReadOnlyCollection<SapPriceResponse>> GetPricesAsync(
        IReadOnlyCollection<SapPriceRequest> rows, CancellationToken cancellationToken = default)
    {
        if (rows.Count == 0)
            return [];

        var result = new List<SapPriceResponse>(rows.Count);

        foreach (var batch in rows.Chunk(BatchSize))
        {
            var batchResult = await ExecuteBatch(batch, cancellationToken);
            result.AddRange(batchResult);
        }

        return result;
    }

    private async Task<ZMV_WS_GET_PRICES_FOR_SPResponse> ExecuteWithRetry(
        ZMV_WS_GET_PRICES_FOR_SPRequest request, CancellationToken cancellationToken)
    {
        var pipeline = _pipelineProvider.GetPipeline("sap-retry");
        return await pipeline.ExecuteAsync(async _ => await _client.GetPricesRawAsync(request), cancellationToken);
    }

    private async Task<List<SapPriceResponse>> ExecuteBatch(SapPriceRequest[] batch, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Timeout);

        try
        {
            var sapRequest = MapRequest(batch);
            var sapResponse = await ExecuteWithRetry(sapRequest, cts.Token);
            return MapResponse(sapResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SAP batch failed, size={Count}", batch.Length);
            throw;
        }
    }

    private static ZMV_WS_GET_PRICES_FOR_SPRequest MapRequest(IReadOnlyCollection<SapPriceRequest> rows)
    {
        return new ZMV_WS_GET_PRICES_FOR_SPRequest(
            new ZMV_WS_GET_PRICES_FOR_SP1
            {
                IT_INPUT = rows.Select(sapPriceRequest =>
                    new ZMVS_GET_PRICES_FOR_SP_INPUT
                    {
                        LIFNR = sapPriceRequest.ServiceCompanySapId,
                        WERKS = sapPriceRequest.ShopName,
                        MATNR = FormatMatnr(sapPriceRequest.WareCode),
                        ZDATE = sapPriceRequest.PriceDate.ToString("yyyy-MM-dd")
                    }).ToArray()
            });
    }

    private static List<SapPriceResponse> MapResponse(ZMV_WS_GET_PRICES_FOR_SPResponse sapResponse)
    {
        if (sapResponse.ET_OUTPUT == null)
            return [];

        return sapResponse.ET_OUTPUT
            .Select(output =>
            {
                var serviceDate = DateOnly.TryParseExact(
                    output.ZDATE, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
                    ? parsedDate : throw new SapInvalidResponseException();

                return new SapPriceResponse
                {
                    ServiceCompanySapId = output.LIFNR,
                    ShopName = output.WERKS,
                    WareCode = NormalizeMatnr(output.MATNR),
                    PriceDate = serviceDate,
                    Price = output.KBETR,
                    Nds = output.CN_NDS
                };
            })
            .ToList();
    }

    // SAP формат
    private static string FormatMatnr(string wareCode) => wareCode.PadLeft(18, '0');

    // обратно к исходному виду
    private static string NormalizeMatnr(string matnr) => matnr.TrimStart('0');
}
