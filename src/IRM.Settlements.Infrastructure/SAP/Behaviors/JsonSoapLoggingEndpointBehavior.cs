using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Extensions.Logging;

namespace IRM.Settlements.Infrastructure.SAP.Behaviors;

public class JsonSoapLoggingEndpointBehavior : IEndpointBehavior
{
    private readonly ILogger _logger;

    public JsonSoapLoggingEndpointBehavior(ILogger logger)
    {
        _logger = logger;
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(new JsonSoapLoggingMessageInspector(_logger));
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

    public void Validate(ServiceEndpoint endpoint) { }
}
