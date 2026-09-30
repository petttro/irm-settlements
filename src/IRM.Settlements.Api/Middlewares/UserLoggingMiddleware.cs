using Serilog.Context;

namespace IRM.Settlements.Api.Middlewares;

public class UserLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserLoggingMiddleware> _logger;

    public UserLoggingMiddleware(RequestDelegate next, ILogger<UserLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var user = context.User.Identity?.Name ?? "anonymous";

        using (LogContext.PushProperty("UserName", user))
        {
            await _next(context);
        }
    }
}
