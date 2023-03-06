using AppSample.Domain.Helpers;
using AppSample.Domain.Models;

namespace AppSample.Domain.DAL.DTOs;

public class AuthorizationRequestDto
{
    public long Id { get; init; }

    public Guid AuthorizationRequestId { get; init; }

    public string? NotificationUri { get; init; }
    public string? NotificationToken { get; init; }

    public int ServiceProviderId { get; init; }
    public string? Scope { get; init; }

    readonly string? _acrValues;
    public string? AcrValues
    {
        get => _acrValues;
        init
        {
            _acrValues = value;
            AcrValuesFlags = _acrValues?.GetAcrValues() ?? DTOs.AcrValues.NoValue;
            MinLoA = AcrValuesFlags.Min();
        }
    }
    public AcrValues AcrValuesFlags { get; private init; }
    public AcrValues MinLoA { get; private init; }
    public string? ResponseType { get; init; }
    public string? Msisdn { get; init; }
    public string? Nonce { get; init; }
    public string? CorrelationId { get; init; }
    public Guid ConsentCode { get; init; }
    public Guid OtpKey { get; init; }

    public decimal? OrderSum { get; init; }

    public DateTime CreatedAt { get; init; }

    public string? RedirectUri { get; init; }
    public string? State { get; init; }
    public MobileIdMode Mode { get; init; }

    public string? Context { get; init; }

    public string? BindingMessage { get; init; }
}

[Flags]
public enum AcrValues
{
    NoValue = 0,
    LoA0 = 1 << 0,
    LoA1 = 1 << 1,
    LoA2 = 1 << 2,
    LoA3 = 1 << 3,
    LoA4 = 1 << 4,
    Supported = LoA2 | LoA3
}