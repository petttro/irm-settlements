using System.Text;
using Serilog.Core;
using Serilog.Events;

namespace IRM.Settlements.Api.Serilog.Enrichers;

public class CategoryNameEnricher : ILogEventEnricher
{
    private const int TrimNamespaceChars = 5;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue("SourceContext", out var sourceContextValue))
            return;

        var sourceContext = sourceContextValue.ToString().Trim('"');

        if (string.IsNullOrWhiteSpace(sourceContext))
            return;

        var parts = sourceContext.Split('.');
        var length = parts.Length;

        if (length < 3)
            return;

        var sb = new StringBuilder(sourceContext.Length);

        for (var i = 0; i < length; i++)
        {
            if (i < length - 2)
            {
                var part = parts[i];
                var len = part.Length > TrimNamespaceChars ? TrimNamespaceChars : part.Length;
                sb.Append(part, 0, len);
            }
            else
            {
                sb.Append(parts[i]);
            }

            if (i < length - 1)
                sb.Append('.');
        }

        var property = propertyFactory.CreateProperty("Category", sb.ToString());
        logEvent.AddPropertyIfAbsent(property);
    }
}
