using IRM.Settlements.Domain.Exceptions;

namespace IRM.Settlements.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureExceptionsHandlers(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            StatusCodeSelector = ex => ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                ApiValidationException => StatusCodes.Status400BadRequest,
                ReportIncorrectStateException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                MissingSettingException => StatusCodes.Status500InternalServerError,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                _ => StatusCodes.Status500InternalServerError
            }
        });

        return app;
    }
}
