using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Models;

public class CreateTokensCommand
{
    public MobileIdMode Mode { get; init; }
    public ServiceProviderEntity ServiceProvider { get; init; }
    public string? Msisdn { get; init; }
    public string? AcrValues { get; init; }
    public string? Nonce { get; init; }
    public string? ResponseType { get; init; }
    public string? NotificationUri { get; init; }
}