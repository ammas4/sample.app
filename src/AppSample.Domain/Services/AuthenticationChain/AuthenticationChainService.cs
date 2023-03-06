using System.Collections.Immutable;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Services.AuthenticationResultHandlers;
using AppSample.Domain.Services.Authenticators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppSample.Domain.Services.AuthenticationChain;

public interface IAuthenticationChainService
{
    /// <summary>
    /// Запускает цепочку аутентификаторов для msisdn
    /// </summary>
    /// <param name="authRequestInput"></param>
    /// <param name="scopes"></param>
    /// <param name="spNotificationToken"></param>
    /// <returns></returns>
    Task<AuthChainStartResult> StartAsync(AuthorizationRequestDto authRequestInput, ImmutableHashSet<string> scopes,
        string? spNotificationToken);

    /// <summary>
    /// Останавливает цепочку аутентификаторов для msisdn
    /// </summary>
    /// <param name="msisdn"></param>
    /// <param name="authenticatorType">Какой аутентификатор обработал</param>
    /// <param name="result"></param>
    /// <returns></returns>
    Task<bool> TryEndAsync(string msisdn, AuthenticatorType authenticatorType, AuthResult result);
}

public class AuthenticationChainServiceService : IAuthenticationChainService
{
    readonly IAuthChainJobScheduler _jobScheduler;
    readonly ICachedConfigService _cachedConfigService;
    readonly IServiceProvider _webAppServiceProvider;
    readonly IAuthenticationChainSession _authenticationChainSession;
    readonly ILogger<AuthenticationChainServiceService> _logger;

    public AuthenticationChainServiceService(
        IAuthChainJobScheduler jobScheduler,
        ICachedConfigService cachedConfigService,
        IServiceProvider webAppServiceProvider,
        IAuthenticationChainSession authenticationChainSession,
        ILogger<AuthenticationChainServiceService> logger
    )
    {
        _jobScheduler = jobScheduler;
        _cachedConfigService = cachedConfigService;
        _webAppServiceProvider = webAppServiceProvider;
        _authenticationChainSession = authenticationChainSession;
        _logger = logger;

        _jobScheduler.HandlerAsync = HandleCurrentAuthenticatorTimeoutAsync;
    }

    public async Task<AuthChainStartResult> StartAsync(AuthorizationRequestDto authRequestInput,
        ImmutableHashSet<string> scopes, string? spNotificationToken)
    {
        if (string.IsNullOrEmpty(authRequestInput.Msisdn))
            throw new ArgumentException(
                $"{nameof(AuthenticationChainServiceService)}.{nameof(StartAsync)}: Msisdn is null.");

        var serviceProviders = _cachedConfigService.GetState().ServiceProvidersById;
        if (!serviceProviders.TryGetValue(authRequestInput.ServiceProviderId, out var serviceProvider))
            return new AuthChainStartResult();

        var (sessionCreated, session) =
            await _authenticationChainSession.TryCreateAsync(authRequestInput.Msisdn, scopes, authRequestInput,
                spNotificationToken);

        if (!sessionCreated)
        {
            var handler = _webAppServiceProvider.GetAuthResultHandler(authRequestInput.Mode);
            await handler.HandleResultAsync(session.Bag, AuthenticatorType.NoValue, AuthResult.PreviousNotFinished);
            return new AuthChainStartResult();
        }

        var (chainStartResult, _) =
            await TryStartNextAvailableAuthenticatorAsync(session, serviceProvider.AuthenticatorChain);
        if (chainStartResult.IsStarted) return chainStartResult;

        await _authenticationChainSession.TryReleaseAsync(session.Msisdn);
        _logger.LogError(new InvalidOperationException(), "Не удалось запустить цепочку аутентификаторов.");
        
        return chainStartResult;
    }

    async Task<(AuthChainStartResult startResult, AuthSessionDto updatedSession)>
        TryStartNextAvailableAuthenticatorAsync(AuthSessionDto session, AuthenticatorChain authenticatorChain)
    {
        authenticatorChain.PreviousStarted = session.PreviousStarted;

        var authenticatorStartResults = new List<AuthenticatorStartResult>();
        foreach (var authenticator in authenticatorChain)
        {
            try
            {
                if (!await _authenticationChainSession.IsAliveAsync(session.Msisdn))
                    return (new AuthChainStartResult { IsStarted = false, StartResults = authenticatorStartResults },
                        session);

                var authStarter = _webAppServiceProvider.GetAuthStarter(authenticator.Type);
                var authenticatorStartResult = await authStarter.TryStartAsync(session.Bag);
                authenticatorStartResults.Add(authenticatorStartResult);

                if (authenticatorStartResult.ResultType != AuthenticatorStartResultType.Started)
                    continue;

                var (isAlive, updatedSession) =
                    await _authenticationChainSession.UpdateAsync(session.Msisdn, authenticator,
                        authenticator.NextChainStartDelay);

                if (!isAlive)
                    return (new AuthChainStartResult { IsStarted = false, StartResults = authenticatorStartResults },
                        session);

                // Важно использовать данные сессии из редиса,
                // Т.к. они могут расходиться с объектом сессии из замыкания state machine async-await.
                // Например, если первый вызов state machine продолжится, после того, как отработает второй,
                // Тогда в замыкании первого state machine будет лежать первый AuthRequest, а в редисе второй
                await _jobScheduler.FireWithDelayAsync(updatedSession.Bag.AuthRequest.AuthorizationRequestId,
                    (int)authenticator.NextChainStartDelay.TotalSeconds);

                return (new AuthChainStartResult { IsStarted = true, StartResults = authenticatorStartResults },
                    updatedSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Запуск {authenticator.Type} завершился исключением.");
            }
        }

        return (new AuthChainStartResult { IsStarted = false, StartResults = authenticatorStartResults }, session);
    }

    public async Task<bool> TryEndAsync(string msisdn, AuthenticatorType authenticatorType, AuthResult result)
    {
        var (isAlive, session) = await _authenticationChainSession.TryGetAsync(msisdn);
        if (!isAlive) return false;

        return await TryEndAsync(session, authenticatorType, result);
    }

    async Task<bool> TryEndAsync(AuthSessionDto sessionDto, AuthenticatorType authenticatorType, AuthResult authResult)
    {
        if (string.IsNullOrEmpty(sessionDto.Msisdn))
            throw new ArgumentException(
                $"{nameof(AuthenticationChainServiceService)}.{nameof(TryEndAsync)}: Msisdn is null.");

        var sessionReleased = await _authenticationChainSession.TryReleaseAsync(sessionDto.Msisdn);
        if (!sessionReleased) return false;

        var handler = _webAppServiceProvider.GetAuthResultHandler(sessionDto.Bag.AuthRequest.Mode);
        await handler.HandleResultAsync(sessionDto.Bag, authenticatorType, authResult);

        return true;
    }

    async Task HandleCurrentAuthenticatorTimeoutAsync(AuthorizationRequestDto authRequestInput)
    {
        if (string.IsNullOrEmpty(authRequestInput.Msisdn))
            throw new ArgumentException(
                $"{nameof(AuthenticationChainServiceService)}.{nameof(HandleCurrentAuthenticatorTimeoutAsync)}: Msisdn is null.");

        var (isAlive, session) = await _authenticationChainSession.TryGetAsync(authRequestInput.Msisdn);
        if (!isAlive) return;

        if (authRequestInput.AuthorizationRequestId != session.Bag.AuthRequest.AuthorizationRequestId) return;

        var serviceProviders = _cachedConfigService.GetState().ServiceProvidersById;
        if (!serviceProviders.TryGetValue(session.Bag.AuthRequest.ServiceProviderId, out var serviceProvider))
            return;

        var (chainStartResult, updatedSession) =
            await TryStartNextAvailableAuthenticatorAsync(session, serviceProvider.AuthenticatorChain);
        if (chainStartResult.IsStarted) return;

        await TryEndAsync(updatedSession, AuthenticatorType.NoValue, AuthResult.Timeout);
    }
}

public static class AuthAuthenticationChainHelper
{
    public static IAuthStarter GetAuthStarter(this IServiceProvider sp, AuthenticatorType type)
    {
        return type switch
        {
            AuthenticatorType.SmsWithUrl => sp.GetRequiredService<ISmsUrlAuthenticator>(),
            AuthenticatorType.SmsOtp => sp.GetRequiredService<ISmsOtpAuthenticator>(),
            AuthenticatorType.Ussd => sp.GetRequiredService<IUssdAuthenticator>(),
            AuthenticatorType.PushMc => sp.GetRequiredService<IMcPushAuthenticator>(),
            AuthenticatorType.PushDstk => sp.GetRequiredService<IDstkPushAuthenticator>(),
            AuthenticatorType.Seamless => sp.GetRequiredService<ISeamlessAuthenticator>(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static IAuthenticationResultHandler GetAuthResultHandler(this IServiceProvider sp, MobileIdMode mode)
    {
        return mode switch
        {
            MobileIdMode.SI => sp.GetRequiredService<ISiAuthenticationResultHandler>(),
            MobileIdMode.DI => sp.GetRequiredService<IDiAuthenticationResultHandler>(),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}