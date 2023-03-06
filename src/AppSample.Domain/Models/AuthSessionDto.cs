using System.Collections.Immutable;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Models;

/// <summary>
/// Модель сессии аутентификации для msisdn
/// </summary>
public struct AuthSessionDto
{
    public AuthSessionDto(AuthSessionDto sessionDto, AuthenticatorEntity previousStarted)
    {
        this = sessionDto;
        PreviousStarted = previousStarted;
    }

    public AuthSessionDto(string msisdn, ImmutableHashSet<string> scopes, AuthorizationRequestDto authRequest,
        string? spNotificationToken)
    {
        PreviousStarted = default;

        Msisdn = msisdn;
        Bag = new AuthSessionBag
        {
            AuthRequest = authRequest,
            Scopes = scopes,
            SpNotificationToken = spNotificationToken
        };
    }

    public string Msisdn { get; init; }
    public AuthenticatorEntity PreviousStarted { get; init; }
    public AuthSessionBag Bag { get; init; }
}

/// <summary>
/// Доп данные, которые нужны для работы аутентификаторам и другим участникам
/// </summary>
public struct AuthSessionBag
{
    public AuthorizationRequestDto AuthRequest { get; init; }
    public ImmutableHashSet<string> Scopes { get; init; }
    public string SpNotificationToken { get; init; }
}