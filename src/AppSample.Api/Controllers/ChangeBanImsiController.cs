using System.Text;
using AppSample.Api.Models;
using AppSample.Domain.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppSample.Api.Controllers;

[ApiController]
public class ChangeBanImsiController : ControllerBase
{
    readonly ChangeBanImsiSettings _settings;
    readonly ISubjectService _subjectService;

    public ChangeBanImsiController(
        IOptions<ChangeBanImsiSettings> settings,
        ISubjectService subjectService)
    {
        _settings = settings.Value;
        _subjectService = subjectService;
    }

    // todo: добавить авторизацию
    [HttpPost("change_ban")]
    public async Task<IActionResult> ChangeBan([FromBody] ChangeBanRequestVm request)
    {
        CheckAuthHeaders();
        CheckParameters(request);

        await _subjectService.DeleteSubjectsForMsisdn(request!.Msisdn!);

        return Ok();
    }

    void CheckAuthHeaders()
    {
        var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
        var byteArrayCredentials = Encoding.UTF8.GetBytes($"{_settings.UserName}:{_settings.Password}");
        var base64Credentials = Convert.ToBase64String(byteArrayCredentials);

        if (authHeader != $"{AuthorizationSchemes.Basic} {base64Credentials}")
        {
            throw new UnifiedException(OAuth2Error.UnauthorizedClient, "Invalid user credentials");
        }
    }

    static void CheckParameters(ChangeBanRequestVm? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Msisdn))
        {
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                "Mandatory parameter is missing or value is invalid");
        }
    }
}
