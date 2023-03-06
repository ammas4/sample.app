using System.Text.Json.Serialization;
using AppSample.Domain.Models;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.Api.Models;

public class SiAuthorizationResponseVm
{
    public SiAuthorizationResponseVm(SiAuthorizationResult source)
    {
        AuthorizationRequestId = source.AuthorizationRequestId;
        ExpiresIn = source.ExpiresIn;
        CorrelationId = source.CorrelationId;
        SmsOtpEnabled = source.SmsOtpEnabled;
        LeadingKycMatch = source.LeadingKycMatch;
        SmsOtpEndpoint = source.SmsOtpEndpoint;
        HheUri = source.HheUri;
    }

    [JsonPropertyName(MobileConnectParameterNames.AuthorizationRequestId)]
    public Guid AuthorizationRequestId { get; set; }

    [JsonPropertyName(OpenIdConnectParameterNames.ExpiresIn)]
    public int ExpiresIn { get; set; }

    [JsonPropertyName(MobileConnectParameterNames.CorrelationId)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CorrelationId { get; set; }

    [JsonPropertyName(MobileConnectParameterNames.SmsOtpEnabled)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SmsOtpEnabled { get; set; }

    [JsonPropertyName(MobileConnectParameterNames.LeadingKycMatch)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? LeadingKycMatch { get; set; }

    [JsonPropertyName(MobileConnectParameterNames.SmsOtpEndpointKey)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SmsOtpEndpoint { get; set; }

    /// <summary>
    /// Ссылка на HHE для бесшовного входа из мобильной сети оператора
    /// </summary>
    [JsonPropertyName(MobileConnectParameterNames.HheUri)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? HheUri { get; set; }
}