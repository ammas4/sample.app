using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.CachedConfig;

public class ConfigState
{
    public List<ServiceProviderEntity> ServiceProviders { get; set; } = new();
    public Dictionary<string, ServiceProviderEntity> ServiceProvidersByClientId { get; set; } = new();
    public Dictionary<int, ServiceProviderEntity> ServiceProvidersById { get; set; } = new();
    public Dictionary<string, string> Settings { get; set; } = new();
    public bool IsMigrationSmsUrlForced { get; set; }
}