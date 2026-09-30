using IRM.Settlements.Infrastructure.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Infrastructure.Common.Http.DelegatingHandlers;

#pragma warning disable CA2254 // Нужно для логирования тела запроса без escape символов

public class LogHttpRequestDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LogHttpRequestDelegatingHandler> _logger;
    private const int MaxChars = 20_000;

    public LogHttpRequestDelegatingHandler(ILogger<LogHttpRequestDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.AbsoluteUri;
        var verb = request.Method.Method;
        var requestBody = string.Empty;

        if (request.Content is not null)
            requestBody = await request.Content.ReadAsStringAsync(cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation($"HTTP request. Verb: {verb}, Url: {url}, RequestBody: {requestBody}");

        var response = await base.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var truncatedResponseBody = responseBody.Truncate(MaxChars);

        var logLevel = LogLevel.Information;

        if (!response.IsSuccessStatusCode)
            logLevel = LogLevel.Error;

        if (_logger.IsEnabled(logLevel))
            _logger.Log(logLevel,
                $"HTTP response. Verb: {verb}, Url: {url}, StatusCode: {(int)response.StatusCode}, " +
                $"RequestBody: {requestBody}, ResponseBody: {truncatedResponseBody} ");

        return response;
    }
}
