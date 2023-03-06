namespace AppSample.Domain.Models.ServiceProviders;

/// <summary>
/// Аутентификаторы (все возможные)
/// </summary>
public enum AuthenticatorType
{
    NoValue = 0,
    Seamless = 1,
    SmsWithUrl = 2,
    SmsOtp = 3,
    Ussd = 4,
    PushMc = 5,
    PushDstk = 6,
}

public static class EnumCollections
{
    /// <summary>
    /// Типы аутентификаторов, подходящие для момента настройки маршрутизации пушей на SMSR во время миграции
    /// </summary>
    public static HashSet<AuthenticatorType> AvailableForForceSmsAuthenticatorTypes => new()
    {
        AuthenticatorType.Seamless,
        AuthenticatorType.SmsWithUrl,
        AuthenticatorType.SmsOtp,
        AuthenticatorType.Ussd
    };
}