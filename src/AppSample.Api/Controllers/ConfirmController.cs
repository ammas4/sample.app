using AppSample.CoreTools.Helpers;
using AppSample.Domain.Extensions;
using AppSample.Domain.Models;
using AppSample.Domain.Models.Confirmation;
using AppSample.Domain.Models.Constants;
using AppSample.Domain.Services.Authenticators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppSample.Api.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
public class ConfirmController : ControllerBase
{
    readonly ISmsUrlAuthenticator _smsUrlAuthenticator;
    readonly IdgwSettings _idgwSettings;

    public ConfirmController(ISmsUrlAuthenticator smsUrlAuthenticator,
		IOptions<IdgwSettings> idgwSettings)
    {
        _smsUrlAuthenticator = smsUrlAuthenticator;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpGet("confirm")]
    public async Task<IActionResult> Get(string? code)
    {
        ConfirmCodeResult result;
        if (string.IsNullOrEmpty(code)
            || StringHelper.TryParseShortGuid(code, out var key) == false
            || (result = await _smsUrlAuthenticator.ProcessConfirmCodeAsync(key)).IsSuccessful == false)
        {
            RemoveCookies();
            //неверный параметр code
            return Redirect("/result/expired");
        }
        SetCookies(result);
        return Redirect("/result/success");
    }

    void SetCookies(ConfirmCodeResult result)
    {
        if( result.AuthorizationRequest != null
            && result.ServiceProvider != null
            && result.AuthorizationRequest.Mode == MobileIdMode.SI
            && !string.IsNullOrWhiteSpace(result.ServiceProvider.Name)
            && !string.IsNullOrWhiteSpace(result.ServiceProvider.AuthPageUrl) )
        {
            Response.SetCookie(Cookies.SpSiteUrlKey, result.ServiceProvider.AuthPageUrl, false);
            Response.SetCookie(Cookies.SpSiteLabelKey, result.ServiceProvider.Name, false);
            Response.SetCookie(Cookies.SpRedirectTimeout, (result.ServiceProvider.RedirectTimeoutSeconds ?? _idgwSettings.DefaultRedirectTimeoutSeconds).ToString(), false);
        }
        else
        {
            RemoveCookies();
        }
    }

    void RemoveCookies()
    {
        Response.Cookies.Delete(Cookies.SpSiteUrlKey);
        Response.Cookies.Delete(Cookies.SpSiteLabelKey);
        Response.Cookies.Delete(Cookies.SpRedirectTimeout);
    }

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="request"></param>
    ///// <returns></returns>
    //[HttpPost("api/v1/confirm")]
    //public async Task<IActionResult> Post([FromBody] ConfirmPostRequest? request)
    //{
    //    if (request == null
    //        || string.IsNullOrEmpty(request.Code)
    //        || StringHelper.TryParseShortGuid(request.Code, out Guid key) == false
    //        || await _confirmService.ProcessConfirmCodeAsync(key) == false)
    //    {
    //        //неверный параметр code
    //        return Conflict(new {message = "Wrong or expired code"});
    //    }

    //    return Content("OK");
    //}
}

