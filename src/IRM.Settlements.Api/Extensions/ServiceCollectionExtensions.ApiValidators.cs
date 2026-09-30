using FluentValidation;
using IRM.Settlements.Api.Contracts.Validation;

namespace IRM.Settlements.Api.Extensions;

public partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ValidationFilter<>).Assembly);
        return services;
    }
}
