using AppSample.Api.Models;
using AppSample.Domain.Services;
using AppSample.Domain.Services.Authenticators;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Api.Controllers;

[ApiController]
public class SmsOtpController : ControllerBase
{
    readonly ISmsOtpAuthenticator _smsOtpAuthenticator;

    public SmsOtpController(ISmsOtpAuthenticator smsOtpAuthenticator)
    {
        _smsOtpAuthenticator = smsOtpAuthenticator;
    }

    [HttpPost("sms_otp/send/{authReqId}")]
    public async Task<IActionResult> SmsSend([FromRoute] string? authReqId, [FromBody] SmsOtpRequestVm req)
    {
        await _smsOtpAuthenticator.ProcessOtpCodeAsync(authReqId, req.verify_code);
        return Ok();
    }

    [HttpPost("sms_otp/send/{authReqId}")]
    [Consumes("application/x-www-form-urlencoded")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> SmsSendForm([FromRoute] string? authReqId, [FromForm] SmsOtpRequestVm req)
    {
        await _smsOtpAuthenticator.ProcessOtpCodeAsync(authReqId, req.verify_code);
        return Ok();
    }
    
}
