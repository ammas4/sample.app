using AppSample.CoreTools.Settings;

namespace AppSample.Domain.Models;

public class UssdSettings : BaseSettings
{
    public string UssdCenterDomain { get; set; }
    public TimeSpan AuthRequestTimeout { get; set; }
}