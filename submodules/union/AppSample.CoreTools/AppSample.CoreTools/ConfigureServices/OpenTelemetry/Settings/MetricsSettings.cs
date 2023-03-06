namespace AppSample.CoreTools.ConfigureServices.OpenTelemetry.Settings;

/// <summary>
/// Настройки метрик
/// </summary>
public class MetricsSettings
{
    /// <summary>
    /// Метрики включены
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// Включить метрики HttpClient
    /// </summary>
    public bool HttpClientInstrumentation { get; set; }
        
    /// <summary>
    /// Включить метрики AspNetCore
    /// </summary>
    public bool AspNetCoreInstrumentation { get; set; }
}