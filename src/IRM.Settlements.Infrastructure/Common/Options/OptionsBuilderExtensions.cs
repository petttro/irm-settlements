using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IRM.Settlements.Infrastructure.Common.Options;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> Required<TOptions>(
        this OptionsBuilder<TOptions> builder,
        Expression<Func<TOptions, object?>> selector,
        string failureMessage = "is required")
        where TOptions : class
    {
        var compiled = selector.Compile();
        var propertyName = GetPropertyName(selector);

        builder.Services.AddSingleton<IValidateOptions<TOptions>>(
            new ValidateOptions<TOptions>(
                builder.Name,
                options =>
                {
                    var value = compiled(options);

                    return value switch
                    {
                        string s => !string.IsNullOrWhiteSpace(s),
                        null => false,
                        _ => true
                    };
                },
                $"{typeof(TOptions).Name}.{propertyName} {failureMessage}"
            ));

        return builder;
    }

    private static string GetPropertyName<TOptions>(Expression<Func<TOptions, object?>> selector)
    {
        if (selector.Body is MemberExpression member)
            return member.Member.Name;

        if (selector.Body is UnaryExpression unary &&
            unary.Operand is MemberExpression member2)
            return member2.Member.Name;

        throw new ArgumentException("Invalid selector expression");
    }
}
