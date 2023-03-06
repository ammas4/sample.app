namespace AppSample.Domain.Models;

public class DiAuthorizationResult
{
    public string RedirectUrl { get; init; }
    public string SessionId { get; init; }
    public string SpSiteUrl { get; init; }
    public string SpSiteLabel { get; init; }
    public string? OtpKey { get; init; }
}