using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Models;

public class McTokenCommand
{
    public string? Code { get; init; }
    public string? RedirectUri { get; init; }
    public string? GrantType { get; init; }
    public AuthInfo? AuthInfo { get; init; }
}