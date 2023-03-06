namespace AppSample.CoreTools.Settings;

public class ProxySettings : BaseSettings, IProxySettings
{
    public string? Address { get; set; }

    public int? Port { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Domain { get; set; }
    public string[]? ExcludeHosts { get; set; }
}