using AppSample.CoreTools.Settings;

namespace AppSample.Domain.Models;

public class HttpRepositoriesSettings : BaseSettings
{
    public HttpRepositorySettings DeviceAdapter { get; init; }
    public HttpRepositorySettings UserProfile { get; init; }
    
}

public class HttpRepositorySettings
{
    public string? Host { get; init; }
    public string? ApiKey { get; init; }
    public string? UserName { get; init; }
    public string? UserPass { get; init; }
}