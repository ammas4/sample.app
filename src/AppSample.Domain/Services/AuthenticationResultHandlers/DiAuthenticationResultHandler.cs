using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Services.AuthenticationResultHandlers;

public interface IDiAuthenticationResultHandler : IAuthenticationResultHandler
{
}

public class DiAuthenticationResultHandler : IDiAuthenticationResultHandler
{
    readonly IDiAuthStateService _diAuthStateService;
    readonly IUpsService _upsService;

    public DiAuthenticationResultHandler(IDiAuthStateService diAuthStateService, IUpsService upsService)
    {
        _diAuthStateService = diAuthStateService;
        _upsService = upsService;
    }

    /// <summary>
    /// Устанавливает состояние auth request.
    /// </summary>
    /// <param name="handleResultBag"></param>
    /// <param name="authenticatorType">Какой аутентификатор обработал</param>
    /// <param name="authResult"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task HandleResultAsync(AuthSessionBag handleResultBag, AuthenticatorType authenticatorType,
        AuthResult authResult)
    {
        switch (authResult)
        {
            case AuthResult.UserAgree:
                //Console.WriteLine("Di Success");
                await ProcessSuccessAsync(handleResultBag.AuthRequest, authenticatorType);
                return;
            case AuthResult.UserDenied:
                //Console.WriteLine("Di UserDenied");
                await ProcessFailureAsync(handleResultBag.AuthRequest, NotificationError.UserDenied);
                return;
            case AuthResult.Timeout:
                //Console.WriteLine("Di Timeout");
                await ProcessFailureAsync(handleResultBag.AuthRequest, NotificationError.Timeout);
                return;
            case AuthResult.PreviousNotFinished:
                //Console.WriteLine("Di PreviousNotFinished");
                await ProcessFailureAsync(handleResultBag.AuthRequest, NotificationError.PreviousNotFinished);
                return;
            case AuthResult.NoValue:
            case AuthResult.RunOutOfAttempts:
            default:
                //Console.WriteLine("Di ArgumentOutOfRangeException");
                throw new ArgumentOutOfRangeException(nameof(authResult), authResult, null);
        }
    }

    async Task ProcessSuccessAsync(AuthorizationRequestDto authReqDto, AuthenticatorType authenticatorType)
    {
        await _diAuthStateService.ProcessConfirmAsync(authReqDto.AuthorizationRequestId, authenticatorType);
        _ = Task.Run(async () => await _upsService.ReportAboutAuthResult(true, authReqDto));
    }

    async Task ProcessFailureAsync(AuthorizationRequestDto authReqDto, NotificationError notificationError)
    {
        if( notificationError == NotificationError.UserDenied )
            await _diAuthStateService.ProcessRejectAsync(authReqDto.AuthorizationRequestId);
        else
            await _diAuthStateService.ProcessTimeoutAsync(authReqDto.AuthorizationRequestId);
        _ = Task.Run(async () => await _upsService.ReportAboutAuthResult(false, authReqDto));
    }
}