using System.Text;
using AppSample.Api.Models;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models.ServiceProviders;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Api.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
public class McTokenController : ControllerBase
{
    readonly IDiAuthorizeService _diAuthorizeService;

    public McTokenController(IDiAuthorizeService iAuthorizeService)
    {
        _diAuthorizeService = iAuthorizeService;
    }

    [HttpPost("mc-token")]
    public async Task<McTokenResultVm> McToken([FromBody] McTokenRequestVm req, BasicAuthHeaderInfo? basicAuth)
    {
        var command = req.ToCommand(basicAuth);
        var result = await _diAuthorizeService.McTokenAsync(command);
        var response = new McTokenResultVm(result);
        return response;
    }

    [HttpPost("mc-token")]
    [Consumes("application/x-www-form-urlencoded")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<McTokenResultVm> McTokenForm([FromForm] McTokenRequestVm req, BasicAuthHeaderInfo? basicAuth)
    {
        var command = req.ToCommand(basicAuth);
        var result = await _diAuthorizeService.McTokenAsync(command);
        var response = new McTokenResultVm(result);
        return response;
    }
}