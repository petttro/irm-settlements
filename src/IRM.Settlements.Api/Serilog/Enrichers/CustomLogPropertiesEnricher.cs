using Serilog.Core;
using Serilog.Events;

namespace IRM.Settlements.Api.Serilog.Enrichers;

public class CustomLogPropertiesEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // User
        if (!logEvent.Properties.TryGetValue("UserName", out var userNameValue))
            return;

        var userProperty = propertyFactory.CreateProperty("User", userNameValue);
        logEvent.AddPropertyIfAbsent(userProperty);

        // TraceId
        if (!logEvent.Properties.TryGetValue("TraceId", out var traceIdValue))
            return;

        var traceIdProperty = propertyFactory.CreateProperty("TraceId", traceIdValue);
        logEvent.AddPropertyIfAbsent(traceIdProperty);
    }
}