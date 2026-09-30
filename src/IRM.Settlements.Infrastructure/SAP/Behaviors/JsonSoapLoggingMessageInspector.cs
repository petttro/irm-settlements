using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Infrastructure.SAP.Behaviors;

#pragma warning disable CA2254 // Нужно для логирования тела запроса без escape символов

public sealed class JsonSoapLoggingMessageInspector : IClientMessageInspector
{
    private readonly ILogger _logger;

    private const int MaxChars = 20_000;

    public JsonSoapLoggingMessageInspector(ILogger logger)
    {
        _logger = logger;
    }

    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        LogMessage("SOAP request. ", ref request);
        return null!;
    }

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        LogMessage("SOAP response. ", ref reply);
    }

    private void LogMessage(string prefix, ref Message message)
    {
        if (!_logger.IsEnabled(LogLevel.Information))
            return;

        var buffer = message.CreateBufferedCopy(64 * 1024);

        var copyForLogging = buffer.CreateMessage();
        message = buffer.CreateMessage();

        var xml = ReadMessageAsTrimmedString(copyForLogging, MaxChars);

        _logger.LogInformation($"{prefix}: {xml}");
    }

    private static string ReadMessageAsTrimmedString(Message message, int maxChars)
    {
        var sb = new StringBuilder(maxChars + 100);

        using var stream = new MemoryStream();
        using var writer = XmlWriter.Create(stream);

        message.WriteMessage(writer);
        writer.Flush();

        stream.Position = 0;

        using var reader = new StreamReader(stream);

        var total = 0;
        var buffer = new char[1024];
        int read;

        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            var remaining = maxChars - total;
            if (remaining <= 0)
                break;

            var toCopy = Math.Min(read, remaining);

            sb.Append(buffer, 0, toCopy);
            total += toCopy;
        }

        if (reader.Peek() >= 0)
        {
            sb.Append("\n...TRUNCATED...");
        }

        return sb.ToString();
    }
}
