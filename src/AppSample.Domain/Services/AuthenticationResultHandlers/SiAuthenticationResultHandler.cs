using AppSample.Domain.DAL;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Services.AuthenticationResultHandlers;

public interface ISiAuthenticationResultHandler : IAuthenticationResultHandler
{
}

public class SiAuthenticationResultHandler : ISiAuthenticationResultHandler
{
    readonly ISmsHttpRepository _smsHttpRepository;
    readonly ICachedConfigService _cachedConfigService;
    readonly IUpsService _upsService;

    public SiAuthenticationResultHandler(ISmsHttpRepository smsHttpRepository, ICachedConfigService cachedConfigService, IUpsService upsService)
    {
        _smsHttpRepository = smsHttpRepository;
        _cachedConfigService = cachedConfigService;
        _upsService = upsService;
    }

    /// <summary>
    /// Отправляет уведомление в Service Provider
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
                //Console.WriteLine("Si Success");
                await ProcessSuccessAsync(handleResultBag.AuthRequest, authenticatorType);
                return;
            case AuthResult.UserDenied:
                //Console.WriteLine("Si UserDenied");
                await ProcessFailureAsync(handleResultBag.AuthRequest,
                    NotificationError.UserDenied);
                return;
            case AuthResult.Timeout:
                //Console.WriteLine("Si Timeout");
                await ProcessFailureAsync(handleResultBag.AuthRequest,
                    NotificationError.Timeout);
                return;
            case AuthResult.PreviousNotFinished:
                //Console.WriteLine("Si PreviousNotFinished");
                await ProcessFailureAsync(handleResultBag.AuthRequest,
                    NotificationError.PreviousNotFinished);
                return;
            case AuthResult.NoValue:
            case AuthResult.RunOutOfAttempts:
            default:
                //Console.WriteLine("Si ArgumentOutOfRangeException");
                throw new ArgumentOutOfRangeException(nameof(authResult), authResult, null);
        }
    }

    async Task ProcessSuccessAsync(AuthorizationRequestDto authReqDto, AuthenticatorType authenticatorType)
    {
        var config = _cachedConfigService.GetState();
        if( config.ServiceProvidersById.TryGetValue(authReqDto.ServiceProviderId, out var serviceProvider) == false )
            return;

        await _smsHttpRepository.NotifyAboutSuccessAsync(authReqDto, authenticatorType, serviceProvider);
        _ = Task.Run(async () => await _upsService.ReportAboutAuthResult(true, authReqDto));
    }

    async Task ProcessFailureAsync(AuthorizationRequestDto authReqDto, NotificationError notificationError)
    {
        await _smsHttpRepository.NotifyAboutFailureAsync(authReqDto, notificationError);
        _ = Task.Run(async () => await _upsService.ReportAboutAuthResult(false, authReqDto));
    }
}