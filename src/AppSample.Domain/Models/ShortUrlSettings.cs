using AppSample.CoreTools.Settings;

namespace AppSample.Domain.Models;

public class ShortUrlSettings : BaseSettings
{
    public string? ServiceUrl { get; set; }
    public TimeSpan? TimeOut { get; set; }
    public string? ApiKey { get; set; }
    public short? ExpirationInDays { get; set; }
}