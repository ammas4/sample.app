using AppSample.Api.Models;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.CoreTools.Helpers;
using AppSample.Domain.Extensions;
using AppSample.Domain.Models.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppSample.Api.Controllers;

[ApiController]
public class DIAuthorizeController : ControllerBase
{
    readonly IDiAuthorizeService _diAuthorizeService;
    readonly IdgwSettings _settings;

    public DIAuthorizeController(IDiAuthorizeService diAuthorizeService, IOptions<IdgwSettings> settings)
    {
        _diAuthorizeService = diAuthorizeService;
        _settings = settings.Value;
    }

    /// <summary>
    /// Di-авторизация
    /// </summary>
    [HttpGet("authorize")]
    public async Task<RedirectResult> DiAuthorize([FromQuery] DiAuthorizationRequestVm request)
    {
        var command = request.ToCommand(GetConfirmationUrl);

        var result = await _diAuthorizeService.DiAuthorizeGenAsync(command);

        Response.SetCookie(Cookies.SpSiteUrlKey, result.SpSiteUrl, false);
        Response.SetCookie(Cookies.SpSiteLabelKey, result.SpSiteLabel, false);
        Response.SetCookie(Cookies.SessionKey, result.SessionId, false);
        Response.SetCookie(Cookies.CookieOtpKey, result.OtpKey ?? "", false);

        return Redirect(result.RedirectUrl);
    }

    string GetConfirmationUrl(IdgwAuthMode authMode, string msisdnStr, string? verificationUrl)
    {
        var urlBuilder = new UrlBuilder(UrlHelper.Combine(_settings.BasePath, "/confirmation"));
        urlBuilder.Query["type"] = GetAuthModeState(authMode);
        urlBuilder.Query["login_hint"] = msisdnStr;
        /*
        if (verificationUrl != null)
        {
            urlBuilder.Query["verification_url"] = verificationUrl;
        }
        */

        return urlBuilder.ToString();
    }

    [HttpGet("check-confirmation")]
    [HttpGet("api/check-confirmation")]
    public async Task<CheckConfirmationResultVm> CheckConfirmation([FromQuery(Name = MobileConnectParameterNames.SessionId)] string sessionId)
    {
        var result = await _diAuthorizeService.CheckConfirmationAsync(sessionId);
        CheckConfirmationResultVm response = new CheckConfirmationResultVm(_settings.BasePath, result);
        return response;
    }

    string GetAuthModeState(IdgwAuthMode mode)
    {
        return mode switch
        {
            IdgwAuthMode.SmsWithUrl => "sms-url",
            IdgwAuthMode.SmsOTP => "sms-otp",
            IdgwAuthMode.Ussd => "ussd",
            IdgwAuthMode.OldMcPush => "push",
            IdgwAuthMode.DstkPush => "push",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }
}