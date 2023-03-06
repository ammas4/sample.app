using System.Text.Json.Serialization;
using AppSample.Domain.Helpers;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Nest;

namespace AppSample.Api.Models;

public class McTokenRequestVm
{
    [JsonPropertyName(OpenIdConnectParameterNames.Code)]
    [FromForm(Name = OpenIdConnectParameterNames.Code)]
    public string? Code { get; init; }

    [JsonPropertyName(OpenIdConnectParameterNames.RedirectUri)]
    [FromForm(Name = OpenIdConnectParameterNames.RedirectUri)]
    public string? RedirectUri { get; init; }

    [JsonPropertyName(OpenIdConnectParameterNames.GrantType)]
    [FromForm(Name = OpenIdConnectParameterNames.GrantType)]
    public string? GrantType { get; init; }

    public McTokenCommand ToCommand(AuthInfo? authInfo)
    {
        McTokenCommand command = new McTokenCommand()
        {
            Code = Code,
            RedirectUri = RedirectUri,
            GrantType = string.IsNullOrEmpty(GrantType) == false ? GrantType : MobileConnectGrantTypes.AuthorizationCode,
            AuthInfo = authInfo
        };
        return command;
    }
}