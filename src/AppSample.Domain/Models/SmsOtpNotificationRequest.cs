using System.Text.Json.Serialization;

namespace AppSample.Domain.Models;

public class SmsOtpNotificationRequest
{
    [JsonPropertyName(MobileConnectParameterNames.AuthorizationRequestId)]
    public Guid AuthReqId { get; init; }
    
    [JsonPropertyName(MobileConnectParameterNames.SmsOtpEndpointKey)]
    public string SmsOtpEndpoint { get; init; }
    
    [JsonPropertyName(MobileConnectParameterNames.CorrelationId)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CorrelationId { get; init; }

    [JsonPropertyName(MobileConnectParameterNames.Send)]
    public SmsOtpNotificationRequestSendInfo Send { get; init; }
}