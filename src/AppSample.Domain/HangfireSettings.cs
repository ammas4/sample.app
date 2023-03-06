using AppSample.CoreTools.Settings;

namespace AppSample.Domain;

public class HangfireSettings : BaseSettings
{
    public bool ShowDashboard { get; set; }
    public int JobsPollingSeconds { get; set; }
    public int WorkerCount { get; set; }

    public string ReloadHttpContentCron { get; set; }
}