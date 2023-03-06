using System.Text.Json.Serialization;
using AppSample.Domain.Models;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.Api.Models;

public class McTokenResultVm
{
    [JsonPropertyName(OpenIdConnectParameterNames.AccessToken)]
    public string AccessToken { get; set; }

    [JsonPropertyName(OpenIdConnectParameterNames.TokenType)]
    public string TokenType { get; set; }

    [JsonPropertyName(OpenIdConnectParameterNames.ExpiresIn)]
    public int ExpiresIn { get; set; }

    [JsonPropertyName(OpenIdConnectParameterNames.Scope)]
    public string Scope { get; set; }

    [JsonPropertyName(MobileConnectParameterNames.CorrelationId)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CorrelationId { get; set; }

    [JsonPropertyName(OpenIdConnectParameterNames.IdToken)]
    public string IdToken { get; set; }

    public McTokenResultVm(McTokenResult result)
    {
        AccessToken = result.AccessToken;
        TokenType = result.TokenType;
        ExpiresIn = result.ExpiresIn;
        Scope = result.Scope;
        CorrelationId = result.CorrelationId;
        IdToken = result.IdToken;
    }
}