namespace AppSample.CoreTools.Settings;

public interface IProxySettings
{
    public string? Address { get; }
    public int? Port { get; }
    public string? UserName { get; }
    public string? Password { get; }
    public string? Domain { get; }
    public string[]? ExcludeHosts { get; }

}