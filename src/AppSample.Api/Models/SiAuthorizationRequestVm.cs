using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.Api.Models;

public record SiAuthorizationRequestVm(
    [FromForm(Name = OpenIdConnectParameterNames.ClientId)]
    string? clientId,
    string? scope,
    string? request,
    [FromForm(Name = OpenIdConnectParameterNames.ResponseType)]
    string? responseType
);