using System.Text.Json.Serialization;

namespace AppSample.Domain.Models;

public class SmsOtpNotificationRequestSendInfo
{
    [JsonPropertyName("verify_code")]
    public string VerifyCode { get; init; }

    [JsonPropertyName("auth_req_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? AuthReqId { get; init; }
}