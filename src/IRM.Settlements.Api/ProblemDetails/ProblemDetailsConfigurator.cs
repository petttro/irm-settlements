using IRM.Settlements.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IRM.Settlements.Api.ProblemDetails;

public static class ProblemDetailsConfigurator
{
    public static void Configure(ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = context =>
        {
            var httpContext = context.HttpContext;
            var exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            context.ProblemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            if (exception is not null)
            {
                context.ProblemDetails.Title = exception.GetType().Name;
                context.ProblemDetails.Detail = exception.Message;
            }

            if (exception is ApiValidationException validationException)
            {
                var validationProblem = new ValidationProblemDetails(validationException.Errors)
                {
                    Title = "Ошибка валидации",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };

                context.ProblemDetails = validationProblem;
            }
        };
    }
}
