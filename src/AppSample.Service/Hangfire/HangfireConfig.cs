using AppSample.Domain;
using AppSample.Domain.Jobs;
using Hangfire;

namespace AppSample.Service.Hangfire;

public class HangfireConfig
{
    public static void Configure(bool isService, HangfireSettings hangfireSettings)
    {
        if (isService)
        {
            RecurringJob.RemoveIfExists(nameof(ReloadHttpContentJob));
            RecurringJob.AddOrUpdate<ReloadHttpContentJob>(nameof(ReloadHttpContentJob),
                x => x.Load(CancellationToken.None),
                hangfireSettings.ReloadHttpContentCron,
                TimeZoneInfo.Local);
        }
    }
}