using AppSample.Api.Models;
using AppSample.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.Api.Controllers;

[ApiController]
public class SIAuthorizeController : ControllerBase
{
    readonly ISIAuthorizeService _siAuthorizeService;

    public SIAuthorizeController(ISIAuthorizeService siAuthorizeService)
    {
        _siAuthorizeService = siAuthorizeService;
    }

    [HttpGet("si-authorize")]
    public async Task<SiAuthorizationResponseVm> SiAuthorize(
        [FromQuery(Name = OpenIdConnectParameterNames.ClientId)] 
        string? clientId,
        string? scope,
        string? request,
        [FromQuery(Name = OpenIdConnectParameterNames.ResponseType)] 
        string? responseType)
    {
        var result = await _siAuthorizeService.SiAuthorizeGen(clientId, scope, request, responseType);
        return new SiAuthorizationResponseVm(result);
    }

    [HttpPost("si-authorize")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<SiAuthorizationResponseVm> SiAuthorize([FromForm] SiAuthorizationRequestVm req)
    {
        var result = await _siAuthorizeService.SiAuthorizeGen(req.clientId, req.scope, req.request, req.responseType);
        return new SiAuthorizationResponseVm(result);
    }
}