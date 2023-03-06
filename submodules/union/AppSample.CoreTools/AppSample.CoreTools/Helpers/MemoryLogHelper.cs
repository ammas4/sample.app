using NLog;
using LogLevel = NLog.LogLevel;

namespace AppSample.CoreTools.Helpers;

public static class MemoryLogHelper
{
    static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static CancellationTokenSource StartPeriodicLog(TimeSpan period)
    {
        if (period <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period));

        CancellationTokenSource cts = new CancellationTokenSource();
        RunLogTask(period, cts.Token);
        return cts;
    }

    public static void StartPeriodicLog(TimeSpan period, CancellationToken ct, LogLevel? logLevel = null)
    {
        if (period <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period));

        RunLogTask(period, ct, logLevel);
    }

    static void RunLogTask(TimeSpan period, CancellationToken ct, LogLevel? logLevel = null)
    {
        Task.Run(async () =>
        {
            try
            {
                while (ct.IsCancellationRequested == false)
                {
                    long memorySize = System.GC.GetTotalMemory(false);
                    Logger.Log(logLevel ?? LogLevel.Debug, $"GetTotalMemory={(memorySize / 1024 / 1024)}MB");
                    await Task.Delay(period, ct);
                }
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception exp)
            {
                Logger.Error(exp, "MemoryLog error");
            }
        }, ct);
    }
}
