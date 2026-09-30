using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IO;

namespace IRM.Settlements.Api.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    private const int MaxBodyLogLength = 4096; // 4KB
    private const int MaxBodyCaptureLength = 1024 * 1024; // 1MB limit for body capture

    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health/live",
        "/health/ready"
    };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _memoryStreamManager = new RecyclableMemoryStreamManager();
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.GetEncodedPathAndQuery();

        if (ExcludedPaths.Contains(path))
        {
            await _next(context);
            return;
        }

        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        // Чтение тела запроса с ограничением размера
        string? requestBody = await ReadRequestBodySafeAsync(context.Request);

        _logger.LogInformation(
            $"HTTP запрос. Verb: {context.Request.Method}, Url: {path}, RequestBody: {TruncateString(requestBody, MaxBodyLogLength)}");

        // Перехватываем response body
        var originalBody = context.Response.Body;
        await using var captureStream = _memoryStreamManager.GetStream();
        context.Response.Body = captureStream;

        try
        {
            await _next(context);
        }
        finally
        {
            try
            {
                captureStream.Position = 0;
                string? responseBody = null;

                if (captureStream.Length > 0)
                {
                    using var reader = new StreamReader(captureStream, leaveOpen: true);
                    responseBody = await reader.ReadToEndAsync();
                }

                captureStream.Position = 0;
                await captureStream.CopyToAsync(originalBody);

                _logger.LogInformation(
                    $"HTTP ответ. Status: {context.Response.StatusCode}, Verb: {context.Request.Method}, Url: {path}, " +
                    $"RequestBody: {TruncateString(requestBody, MaxBodyLogLength)}, " +
                    $"ResponseBody: {TruncateString(responseBody, MaxBodyLogLength)}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при логировании тела ответа для {Path}", path);

                // В случае ошибки пытаемся скопировать что есть в оригинальный поток
                captureStream.Position = 0;
                await captureStream.CopyToAsync(originalBody);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
    }

    private async Task<string?> ReadRequestBodySafeAsync(HttpRequest request)
    {
        if (request.ContentLength == null || request.ContentLength == 0 || !request.Body.CanRead)
            return null;

        // Защита от слишком больших тел запросов
        if (request.ContentLength > MaxBodyCaptureLength)
        {
            request.EnableBuffering();
            return $"[Тело запроса превышает {MaxBodyCaptureLength} байт]";
        }

        try
        {
            request.EnableBuffering();

            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при чтении тела запроса");
            return "[Ошибка чтения тела запроса]";
        }
    }

    private static string TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return "[empty]";

        return value.Length <= maxLength
            ? value
            : string.Concat(value.AsSpan(0, maxLength), "... [обрезано]");
    }
}
