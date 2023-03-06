using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.Domain.Models;

public class NotificationRequestVm
{
    [JsonPropertyName(MobileConnectParameterNames.AuthorizationRequestId)]
    public Guid AuthorizationRequestId { get; init; }

    [JsonPropertyName(OpenIdConnectParameterNames.AccessToken)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccessToken { get; init; }

    [JsonPropertyName(OpenIdConnectParameterNames.IdToken)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IdToken { get; init; }

    [JsonPropertyName(OpenIdConnectParameterNames.TokenType)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TokenType { get; init; }

    [JsonPropertyName(OpenIdConnectParameterNames.ExpiresIn)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ExpiresIn { get; init; }

    [JsonPropertyName(MobileConnectParameterNames.CorrelationId)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CorrelationId { get; set; }

    [JsonPropertyName(MobileConnectParameterNames.PremiumInfoEndpointKey)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PremiumInfoEndpoint { get; init; }

    [JsonPropertyName(MobileConnectParameterNames.LeadingKycMatch)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? LeadingKycMatch { get; init; }

    [JsonPropertyName(MobileConnectParameterNames.KycMatchEndpointKey)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KycMatchEndpoint { get; init; }

    [JsonPropertyName(MobileConnectParameterNames.JwksEndpointKey)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? JwksEndpoint { get; init; }

    [JsonPropertyName(OpenIdConnectParameterNames.Error)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; init; }

    [JsonPropertyName(OpenIdConnectParameterNames.ErrorDescription)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorDescription { get; init; }
}