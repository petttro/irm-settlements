using FluentValidation;
using IRM.Settlements.Domain.Exceptions;

namespace IRM.Settlements.Api.Contracts.Validation;

public class ValidationFilter<T> : IEndpointFilter
{
    private readonly IValidator<T>? _validator;

    public ValidationFilter(IValidator<T>? validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (_validator is null)
            return await next(context);

        var argument = context.Arguments
            .OfType<T>()
            .FirstOrDefault();

        if (argument is null)
            return await next(context);

        var result = await _validator.ValidateAsync(argument);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            throw new ApiValidationException(errors);
        }

        return await next(context);
    }
}
