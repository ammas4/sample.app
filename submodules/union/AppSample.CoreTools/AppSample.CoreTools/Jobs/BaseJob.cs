using System.Diagnostics;
using AppSample.CoreTools.Logging;
using Microsoft.Extensions.Logging;

namespace AppSample.CoreTools.Jobs;

public abstract class BaseJob
{
    readonly ILogger _logger;

    protected BaseJob(ILogger logger)
    {
        _logger= logger;
    }

    protected async Task Execute(CancellationToken cancellationToken, string jobName, Action callback)
    {
        await ExecuteAsync(cancellationToken, jobName, () => Task.Run(callback));
    }

    protected async Task ExecuteAsync(CancellationToken cancellationToken, string jobName, Func<Task>? callback)
    {
        LogHelper.SetActivityId();

        var time = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation($"Start job with name '{jobName}'");

            cancellationToken.ThrowIfCancellationRequested();
            if (callback != null)
            {
                await callback.Invoke();
            }
            else
            {
                _logger.LogError($"Empty callback for job with name '{jobName}' passed");
            }

            time.Stop();

            _logger.LogInformation($"Finish job with name '{jobName}'; processingTimespan='{time.Elapsed}'");
        }
        catch (OperationCanceledException)
        {
            time.Stop();
            _logger.LogWarning($"Job with name '{jobName}' was canceled; processingTimespan='{time.Elapsed}'");
        }
        catch (Exception ex)
        {
            time.Stop();
            _logger.LogError(ex, $"Job with name '{jobName}' was canceled with error; processingTimespan='{time.Elapsed}'");
            throw;
        }
    }
}