using System.Text.Json.Serialization;
using AppSample.CoreTools.Exceptions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.CoreTools.Contracts;

public class ErrorResult
{
    [JsonPropertyName(OpenIdConnectParameterNames.Error)]
    public string Error { get; set; } = "server_error";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName(OpenIdConnectParameterNames.ErrorDescription)]
    public string? ErrorDescription { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("retry_count")]
    public int? RetryCount { get; set; }

    public ErrorResult(UnifiedException ex)
    {
        Error = OAuth2ErrorDetails.GetText(ex.Error);
        RetryCount = ex.RetryCount;
        ErrorDescription = ex.ErrorDescription;
    }
}