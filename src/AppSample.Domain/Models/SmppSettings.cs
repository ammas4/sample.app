using AppSample.CoreTools.Settings;

namespace AppSample.Domain.Models;

public class SmppSettings : BaseSettings
{
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? From { get; set; }
    public string? SystemId { get; set; }
    public string? Password { get; set; }
}