using AppSample.CoreTools.Settings;

namespace AppSample.CoreTools.ConfigureServices.OpenTelemetry.Settings
{
    public class TelemetrySettings : BaseSettings
    {
        public string Source { get; set; }
        public TracingSettings Tracing { get; set; }
        public MetricsSettings Metrics { get; set; }
    }
}
