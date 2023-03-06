namespace AppSample.Domain.Models;

public class SiAuthorizationResult
{
    public SiAuthorizationResult(Guid authorizationRequestId, int expiresIn, string? correlationId, bool? smsOtpEnabled, bool? leadingKycMatch, string? smsOtpEndpoint, string? hheUri)
    {
        AuthorizationRequestId = authorizationRequestId;
        ExpiresIn = expiresIn;
        CorrelationId = correlationId;
        SmsOtpEnabled = smsOtpEnabled;
        LeadingKycMatch = leadingKycMatch;
        SmsOtpEndpoint = smsOtpEndpoint;
        HheUri = hheUri;
    }

    public Guid AuthorizationRequestId { get; set; }

    public int ExpiresIn { get; set; }

    public string? CorrelationId { get; set; }

    public bool? SmsOtpEnabled { get; set; }
    
    public bool? LeadingKycMatch { get; set; }

    public string? SmsOtpEndpoint { get; set; }

    /// <summary>
    /// Ссылка на HHE для бесшовного входа из мобильной сети оператора
    /// </summary>
    public string? HheUri { get; set; }
}