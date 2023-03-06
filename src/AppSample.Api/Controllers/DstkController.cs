using AppSample.Api.Models;
using AppSample.Domain.Services.Authenticators;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Api.Controllers;

/// <summary>
/// Контроллер для приёма ответа dstk-апплета
/// </summary>
public class DstkController : Controller
{
    /// <summary>
    /// Путь для приёма ответа на пуш
    /// </summary>
    public const string PushAnswerPath = "dstk/push/answer";
    
    /// <summary>
    /// Путь для приёма ответа на пуш с пином
    /// </summary>
    public const string PushPinAnswerPath = "dstk/push-pin/answer";

    readonly IDstkPushAuthenticator _dstkPushAuthenticator;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oldOldMcPushAuthenticator"></param>
    public DstkController(IDstkPushAuthenticator dstkPushAuthenticator)
    {
        _dstkPushAuthenticator = dstkPushAuthenticator;
    }

    /// <summary>
    /// Принимает ответ на пуш
    /// </summary>
    /// <param name="answerInput"></param>
    /// <returns></returns>
    [HttpPost(PushAnswerPath)]
    public async Task<IActionResult> PushAnswer([FromBody] DstkPushAnswerInput answerInput)
    {
        await _dstkPushAuthenticator.ProcessPushAnswer(answerInput.Msisdn, answerInput.Consent);
        return new EmptyResult();
    }

    /// <summary>
    /// Принимает ответ на пуш с пин-кодом
    /// </summary>
    /// <param name="answerInput"></param>
    /// <returns></returns>
    [HttpPost(PushPinAnswerPath)]
    public async Task<IActionResult> PushPinAnswer([FromBody] PushPinAnswerInput answerInput)
    {
        switch (answerInput.VerifyState)
        {
            case PushPinAnswerState.AcceptedByUser:
                await _dstkPushAuthenticator.ProcessPushAnswer(answerInput.Msisdn, true);
                break;
            case PushPinAnswerState.UserBlocked:
            case PushPinAnswerState.DeniedByUser:
                await _dstkPushAuthenticator.ProcessPushAnswer(answerInput.Msisdn, false);
                break;
            case PushPinAnswerState.NoValue:
            case PushPinAnswerState.Timeout:
            default:
                return new EmptyResult();
        }

        return new EmptyResult();
    }
}