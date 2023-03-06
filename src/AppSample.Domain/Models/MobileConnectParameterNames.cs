namespace AppSample.Domain.Models;

public class MobileConnectParameterNames
{
    public const string AuthorizationRequestId = "auth_req_id";

    public const string ClientName = "client_name";

    public const string ClientNotificationToken = "client_notification_token";

    public const string NotificationUri = "notification_uri";

    public const string PremiumInfoEndpointKey = "premiuminfo_endpoint";

    public const string SIAuthorizationEndpointKey = "si_authorization_endpoint";

    public const string Recipient = "recipient";

    public const string Request = "request";

    public const string Version = "version";

    public const string Context = "context";

    public const string BindingMessage = "binding_message";

    public const string McClaims = "mc_claims";

    public const string SmsOtpEnabled = "smsotp_enabled";

    public const string LeadingKycMatch = "leading_kyc_match";

    public const string SmsOtpEndpointKey = "smsotp_endpoint";

    public const string KycMatchEndpointKey = "kyc_match_endpoint";

    public const string JwksEndpointKey = "jwks_endpoint";

    public const string Email = "email";

    public const string LastName = "last_name";

    public const string FirstName = "first_name";

    public const string MiddleName = "middle_name";

    public const string AdvertisingConsent = "adv_consent";

    public const string CorrelationId = "correlation_id";

    public const string OrderSum = "order_sum";

    public const string ConfirmStatus = "confirm_status";

    public const string AutodetectSourceIp = "autodetect_source_ip";

    // Поделки :
    /// <summary>
    /// время, сгенерированное СП, перед отправкой в /si-authorize агрегатора. Формат TimeStamp (MOBID-1950)
    /// </summary>
    public const string SpHitTs = "sp_hit_ts";

    /// <summary>
    /// время, сгенерированное агрегатором для отправки в Idgw-Билайн. Совпадает с SIAuthorizationRequest.CreatedAt. Формат TimeStamp (MOBID-1950)
    /// </summary>
    public const string AggInTs = "agg_in_ts";

    /// <summary>
    /// время, сгенерированное агрегатором для отправки в Idgw-Билайн. Совпадает с SIAuthorizationRequest.IdgwSiAuthReqAt. Формат TimeStamp (MOBID-1950)
    /// </summary>
    public const string AggOutTs = "agg_out_ts";

    public const string SessionId = "session_id";

    /// <summary>
    /// Статичная json-структура данных, ожидаемая при обращении на sms_otp endpoint
    /// </summary>
    public const string Send = "send";

    /// <summary>
    /// Имя поля со ссылкой на HHE для бесшовного входа из мобильной сети оператора
    /// </summary>
    public const string HheUri = "hhe_uri";
}