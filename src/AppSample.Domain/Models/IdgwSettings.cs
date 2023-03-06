using AppSample.CoreTools.Settings;

namespace AppSample.Domain.Models;

public class IdgwSettings : BaseSettings
{
    public PrivateKeySettings PrivateKey { get; set; }
    public PublicKeySettings PublicKey { get; set; }
    public string ConsentMethod { get; set; }
    public string ConsentPayMethod { get; set; }
    public string ConsentText { get; set; }
    public string OtpSmsMessage { get; set; }
    public string PushMessage { get; set; }
    public int OtpAttemptsLimit { get; set; }
    public TimeSpan OtpSessionTimeout { get; set; }
    public TimeSpan SmsUrlCodeTimeout { get; set; }
    public TimeSpan SmsUrlPageAvailability { get; set; }
    public int OtpCodeLength { get; set; }
    public bool? UseDefaultOtpCode { get; set; }
    public string? DefaultOtpCode { get; set; }
    public TimeSpan[]? OtpFirstNotifyDelays { get; set; }
    public string BasePath { get; set; }
    public string TokenIssuer { get; set; }
    public int AuthorizationRequestExpiresInSec { get; set; }
    public int AccessTokenDefaultLifetimeSec { get; set; }
    public int IdentityTokenDefaultLifetimeSec { get; set; }
    public string[] AllowedRequestTokenIssuers { get; set; }
    public int DefaultRedirectTimeoutSeconds { get; set; }
    public int DefaultNextChainStartDelaySeconds { get; set; }
    public string BeelineIpRange { get; set; }
    public string XbrUrl { get; set; }
    public TimeSpan HheLinkTimeout { get; set; }
    public string UsssUrl { get; set; }
    public string BasePathAliasForXbr { get; set; }
}

public class PrivateKeySettings
{
    public string Sig { get; set; }
}

public class PublicKeySettings
{
    public string Enc { get; set; }
}