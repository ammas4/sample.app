using AppSample.Api.Models;
using AppSample.Domain.Services.Authenticators;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Api.Controllers;

/// <summary>
/// Контроллер для приёма ответа mc-апплета версии 1.0
/// </summary>
public class McController : Controller
{
    /// <summary>
    /// Путь для приёма ответа на пуш
    /// </summary>
    public const string PushAnswerPath = "mc/push/answer";
    
    /// <summary>
    /// Путь для приёма ответа на пуш с пином
    /// </summary>
    public const string PushPinAnswerPath = "mc/push-pin/answer";

    readonly IMcPushAuthenticator _mcPushAuthenticator;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mcPushAuthenticator"></param>
    public McController(IMcPushAuthenticator mcPushAuthenticator)
    {
        _mcPushAuthenticator = mcPushAuthenticator;
    }

    /// <summary>
    /// Принимает ответ на пуш
    /// </summary>
    /// <param name="answerInput"></param>
    /// <returns></returns>
    [HttpPost(PushAnswerPath)]
    public async Task<IActionResult> PushAnswer([FromBody] McPushAnswerInput answerInput)
    {
        switch (answerInput.Answer)
        {
            case OldMcPushAnswerType.UserAgree:
                await _mcPushAuthenticator.ProcessPushAnswer(answerInput.Msisdn, true);
                break;
            case OldMcPushAnswerType.UserDenied:
                await _mcPushAuthenticator.ProcessPushAnswer(answerInput.Msisdn, false);
                break;
            case OldMcPushAnswerType.NoResult:
            default:
                return new EmptyResult();
        }
        
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
                await _mcPushAuthenticator.ProcessPushAnswer(answerInput.Msisdn, true);
                break;
            case PushPinAnswerState.UserBlocked:
            case PushPinAnswerState.DeniedByUser:
                await _mcPushAuthenticator.ProcessPushAnswer(answerInput.Msisdn, false);
                break;
            case PushPinAnswerState.NoValue:
            case PushPinAnswerState.Timeout:
            default:
                return new EmptyResult();
        }

        return new EmptyResult();
    }
}