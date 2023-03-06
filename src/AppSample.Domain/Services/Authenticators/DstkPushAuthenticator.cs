using System.Collections.Immutable;
using AppSample.CoreTools.Extensions;
using AppSample.Domain.DAL;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.DeviceAdapter;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Models.Ups;
using AppSample.Domain.Services.AuthenticationChain;
using Microsoft.Extensions.Options;

namespace AppSample.Domain.Services.Authenticators;

public interface IDstkPushAuthenticator : IAuthStarter
{
    Task ProcessPushAnswer(long msisdn, bool consist);
}

public class DstkPushAuthenticator : IDstkPushAuthenticator
{
    readonly IDeviceAdapterRepository _deviceAdapterRepository;
    readonly IAuthenticationChainService _authenticationChainService;
    readonly IUserProfileRepository _userProfileRepository;
    readonly ICachedConfigService _cachedConfigService;
    readonly IdgwSettings _idgwSettings;
    readonly IBaseMetricsService _metricsService;

    public DstkPushAuthenticator(
        IDeviceAdapterRepository deviceAdapterRepository,
        IAuthenticationChainService authenticationChainService,
        IUserProfileRepository userProfileRepository,
        ICachedConfigService cachedConfigService,
        IOptions<IdgwSettings> idgwSettings,
        IBaseMetricsService metricsService
    )
    {
        _deviceAdapterRepository = deviceAdapterRepository;
        _authenticationChainService = authenticationChainService;
        _userProfileRepository = userProfileRepository;
        _cachedConfigService = cachedConfigService;
        _metricsService = metricsService;
        _idgwSettings = idgwSettings.Value;
    }

    public async Task<AuthenticatorStartResult> TryStartAsync(AuthSessionBag startBag)
    {
        if (startBag.AuthRequest.MinLoA == AcrValues.LoA4)
            return StartResult(AuthenticatorStartResultType.LoA4NotSupported);

        if (!long.TryParse(startBag.AuthRequest.Msisdn, out var msisdn))
            return StartResult(AuthenticatorStartResultType.MsisdnInvalid);

        var serviceProviders = _cachedConfigService.GetState().ServiceProvidersById;
        if (!serviceProviders.TryGetValue(startBag.AuthRequest.ServiceProviderId, out var serviceProvider))
            return StartResult(AuthenticatorStartResultType.ServiceProviderNotFound);

        var userProfile = await _userProfileRepository.GetUserProfile(msisdn);
        if (userProfile == null) return StartResult(AuthenticatorStartResultType.UserInfoNotFound);
        if (userProfile.IsBlocked) return StartResult(AuthenticatorStartResultType.UserBlocked);
        if (!userProfile.HasDstkApplet) return StartResult(AuthenticatorStartResultType.SimCardNotSupport);

        var result = await SendPushAsync(msisdn, startBag, serviceProvider);

        if (result.ResultType == AuthenticatorStartResultType.Started)
            _metricsService.AddPushRequest(serviceProvider.Name!);

        return result;
    }

    async Task<AuthenticatorStartResult> SendPushAsync(long msisdn, AuthSessionBag startBag, ServiceProviderEntity sp)
    {
        var message = CreateMessage(startBag.AuthRequest, startBag.Scopes, sp);

        return startBag.AuthRequest.MinLoA switch
        {
            AcrValues.LoA2 => await SendPushLoA2Async(msisdn, message),
            AcrValues.LoA3 => await SendPushLoA3Async(msisdn, message),
            _ => StartResult(AuthenticatorStartResultType.LoA4NotSupported)
        };
    }

    async Task<AuthenticatorStartResult> SendPushLoA2Async(long msisdn, string message)
    {
        var sendResult = await _deviceAdapterRepository.SendPushToDstk(msisdn, message);
        var startResult = sendResult switch
        {
            DaCommandResultType.Sent => StartResult(AuthenticatorStartResultType.Started, AcrValues.LoA2),
            _ => StartResult(AuthenticatorStartResultType.NotSent, AcrValues.LoA2),
        };
        return startResult;
    }

    async Task<AuthenticatorStartResult> SendPushLoA3Async(long msisdn, string message)
    {
        var sendResult = await _userProfileRepository.SendDstkPushPin(msisdn, message);
        return StartResult(sendResult.ToAuthenticatorStartResultType(), AcrValues.LoA3);
    }

    static AuthenticatorStartResult StartResult(AuthenticatorStartResultType resultType,
        AcrValues startedLoA = AcrValues.NoValue) => new()
    {
        Authenticator = AuthenticatorType.PushDstk,
        ResultType = resultType,
        StartedLoA = startedLoA
    };

    public async Task ProcessPushAnswer(long msisdn, bool consist)
    {
        await _authenticationChainService.TryEndAsync(msisdn.ToString(), AuthenticatorType.PushDstk,
            consist ? AuthResult.UserAgree : AuthResult.UserDenied);
    }

    string CreateMessage(AuthorizationRequestDto authRequest, ImmutableHashSet<string> requestedScopes,
        ServiceProviderEntity sp)
    {
        if (sp.Scopes?.IsCustomMessageRequired(requestedScopes, out var message) == true && message != null)
            return message.Replace("%ClientName%", sp.Name);

        var isAuthZScopeSelected = ScopesHelper.HasAuthzScope(requestedScopes);
        if (authRequest.Context is { Length: > 0 } && authRequest.BindingMessage is null or "")
            return isAuthZScopeSelected
                ? authRequest.Context.Truncate(64)
                : authRequest.Context.Truncate(90);

        if (authRequest.Context is null or "" && authRequest.BindingMessage is { Length: > 0 })
            return isAuthZScopeSelected
                ? authRequest.BindingMessage.Truncate(32)
                : authRequest.BindingMessage.Truncate(54);

        if (authRequest.Context is { Length: > 0 } && authRequest.BindingMessage is { Length: > 0 })
            return $"{authRequest.BindingMessage} {authRequest.Context}".Truncate(89);

        return $"{_idgwSettings.PushMessage} в {sp.Name}";
    }
}