using AppSample.CoreTools.Helpers;
using AppSample.Domain;
using AppSample.Service.Hangfire;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppSample.Service.Services;

public class JobService : IHostedService, IDisposable
{
    readonly ILogger<JobService> _logger;

    readonly HangfireSettings _hangfireSettings;
    readonly IServiceProvider _serviceProvider;
    BackgroundJobServer? _backgroundJobServer;

    public JobService(IServiceProvider serviceProvider, HangfireSettings hangfireSettings)
    {
        _serviceProvider = serviceProvider;
        _hangfireSettings = hangfireSettings;
        _logger = serviceProvider.GetRequiredService<ILogger<JobService>>();
    }

    public void Start()
    {
        AsyncHelper.RunSync(() => StartAsync(CancellationToken.None));
    }

    public void Stop()
    {
        AsyncHelper.RunSync(() => StopAsync(CancellationToken.None));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("JobsService Start()");

        // Configure hangfire to use the new JobActivator we defined.
        var hangfireActivatorLogger = _serviceProvider.GetRequiredService<ILogger<HangfireActivator>>();
        var hangfireActivator = new HangfireActivator(_serviceProvider, hangfireActivatorLogger);
        GlobalConfiguration.Configuration.UseActivator(hangfireActivator);
        HangfireConfig.Configure(true, _hangfireSettings);

        _backgroundJobServer = new BackgroundJobServer(new BackgroundJobServerOptions
        {
            SchedulePollingInterval = TimeSpan.FromSeconds(_hangfireSettings.JobsPollingSeconds),
            WorkerCount = _hangfireSettings.WorkerCount
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("JobsService Stop()");

        _backgroundJobServer?.SendStop();
        _backgroundJobServer?.Dispose();
        _backgroundJobServer = null;

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _backgroundJobServer?.SendStop();
        _backgroundJobServer?.Dispose();
    }
}