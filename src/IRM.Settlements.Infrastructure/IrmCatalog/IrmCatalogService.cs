using IRM.Settlements.Application.Abstractions.IrmCatalog;
using IRM.Settlements.Domain.Exceptions;
using IRM.Settlements.Domain.ValueObjects;
using IRM.Settlements.Infrastructure.IrmCatalog.HttpClients;

namespace IRM.Settlements.Infrastructure.IrmCatalog;

public class IrmCatalogService : IIrmCatalogService
{
    private readonly IrmCatalogHttpClient _httpClient;

    public IrmCatalogService(IrmCatalogHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ServiceCompanyDetails> GetServiceCompanyDetailsAsync(
        string serviceCompanySapId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetServiceCompanyAsync(serviceCompanySapId, cancellationToken);
        if (response is null)
            throw new NotFoundException($"Сервисная компания {serviceCompanySapId} не найдена в сервисе каталогов");

        return response.ToEntity();
    }

    public async Task<List<ServiceCompanyDetails>> GetServiceCompaniesDetailsAsync(
        List<string> serviceCompanySapIds, CancellationToken cancellationToken = default)
    {
        var request = new ServiceCompanyQueryRequest(serviceCompanySapIds);
        var response = await _httpClient.QueryServiceCompaniesAsync(request, cancellationToken);

        if (response is null)
            throw new NotFoundException("Сервис каталогов вернул недопустимый ответ");

        var missingSapIds = serviceCompanySapIds.Except(response.Data.Select(s => s.SapId)).ToList();
        if (missingSapIds.Count != 0)
            throw new NotFoundException($"Сервисные компании не найдены в сервисе каталогов: [{string.Join(", ", missingSapIds)}] ");

        return response.Data
            .Select(sc => sc.ToEntity())
            .ToList();
    }
}
