using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppSample.Service.Hangfire;

public class HangfireActivator : JobActivator
{
    readonly ILogger<HangfireActivator> _logger;

    readonly IServiceProvider _serviceProvider;

    public HangfireActivator(IServiceProvider serviceProvider,ILogger<HangfireActivator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override object ActivateJob(Type type)
    {
        try
        {
            var res = _serviceProvider.GetRequiredService(type);
            return res;
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "HangfireActivator.ActivateJob() failed");
            throw;
        }
    }
}