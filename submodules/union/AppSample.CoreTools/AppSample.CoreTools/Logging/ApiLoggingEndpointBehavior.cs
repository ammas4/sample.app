using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace AppSample.CoreTools.Logging;

public class ApiLoggingEndpointBehavior : IEndpointBehavior
{
    readonly IRequestResponseLogger _requestResponseLogger;
    readonly string _clientName;

    public ApiLoggingEndpointBehavior(IRequestResponseLogger requestResponseLogger, string clientName)
    {
        _requestResponseLogger = requestResponseLogger;
        _clientName = clientName;
    }

    public void Validate(ServiceEndpoint endpoint)
    {
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
    {
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(new ApiLoggingMessageInspector(_requestResponseLogger, _clientName));
    }
}