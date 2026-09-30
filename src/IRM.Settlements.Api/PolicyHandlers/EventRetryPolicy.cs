using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.Core.Reflection;
using Wolverine.Configuration;
using Wolverine.ErrorHandling;
using Wolverine.Runtime.Handlers;

namespace IRM.Settlements.Api.PolicyHandlers;

/// <summary>
/// Политики повторной обработки событий для Wolverine.
/// </summary>
public class EventRetryPolicy : IHandlerPolicy
{
    private readonly TimeSpan _initial;
    private readonly TimeSpan _increment;
    private readonly TimeSpan _max;

    /// <summary>
    /// Политики повторной обработки событий для Wolverine.
    /// </summary>
    /// <param name="initial">Начало счетчика.</param>
    /// <param name="increment">Инкремент счетчика.</param>
    /// <param name="max">Максимальное значение счетчика.</param>
    public EventRetryPolicy(TimeSpan initial, TimeSpan increment, TimeSpan max)
    {
        _initial = initial;
        _increment = increment;
        _max = max;
    }

    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, IServiceContainer container)
    {
        // Получаем все доменные события.
        var eventChains = chains
            .Where(x => x.MessageType.IsInNamespace("IRM.Settlements.Application.Events"));

        // Для каждого события устанавливаем retry policy.
        foreach (var chain in eventChains)
        {
            chain.OnException<Exception>()
                .RetryWithCooldown(_initial, _increment, _max)
                .Then
                .MoveToErrorQueue();
        }
    }
}
