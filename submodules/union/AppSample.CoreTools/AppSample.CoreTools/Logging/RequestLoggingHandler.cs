using AppSample.CoreTools.Helpers.BatchProcessor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppSample.CoreTools.Logging;

public class RequestLoggingHandler: DelegatingHandler, IDisposable
{
    private readonly ILogger<RequestLoggingHandler> _logger;
    private readonly BatchProcessor<RequestEvent> _batchProcessor;
    
    public RequestLoggingHandler(
        ILogger<RequestLoggingHandler> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        IRequestLoggingBatchProcessor batchSaveProcessor)
    {
        _logger = logger;
        _batchProcessor = new BatchProcessor<RequestEvent>(
            logger, 
            "RequestEventSave", 
            TimeSpan.FromMinutes(1), 
            10, 
            TimeSpan.FromMinutes(1), 
            100, 
            batchSaveProcessor.ProcessBatchSave);
        
        hostApplicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;
        
        var requestEvent = new RequestEvent
        {
            CreateDate = now,
            StartDate = now,
            TransactionId = 1
        };

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            
            requestEvent.EndDate = DateTimeOffset.Now;
            
            return response;
        }
        finally
        {
            await _batchProcessor.Schedule(requestEvent);
        }
    }

    public void Dispose()
    {
        _batchProcessor.Stop();
    }
    
    void OnApplicationStopping()
    {
        _batchProcessor.Stop();
    }
}