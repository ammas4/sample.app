using System.Collections.Immutable;

namespace AppSample.Domain.Models;

public class SiRequestValidationInfo
{
    public string Msisdn { get; init; }
    public string AcrValues { get; init; }
    public string? SpHitTs { get; init; }
    public string? ResponseType { get; init; }
    public string SPNotificationUri { get; init; }
    public string? Nonce { get; set; }
    public string? CorrelationId { get; set; }

    public string SPNotificationToken { get; init; }
    public ImmutableHashSet<string> Scopes { get; init; }
    public string? Context { get; init; }
    public decimal? OrderSum { get; init; }

    public string? BindingMessage { get; init; }
}