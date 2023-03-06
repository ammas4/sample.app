using AppSample.CoreTools.Settings;

namespace AppSample.CoreTools.ConfigureServices.OpenTelemetry.Settings;

public class JaegerSettings : BaseSettings
{
    public string AgentHost { get; set; }
    public int AgentPort { get; set; }
}
