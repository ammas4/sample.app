using System.Diagnostics;
using System.Net;
using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using AppSample.CoreTools.Redis;
using AppSample.Domain.DAL;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.Confirmation;
using AppSample.Domain.Models.Constants;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Services.AuthenticationChain;
using LanguageExt.Common;
using Microsoft.Extensions.Options;

namespace AppSample.Domain.Services.Authenticators;

public interface ISmsUrlAuthenticator : IAuthStarter
{
    Task<Result<PaymentInfoResult>> GetRequestInfoByConfirmCodeAsync(Guid key);
    Task<Result<bool>> ProcessConfirmCodeAsync(Guid key, ConfirmationReason reason);
    Task<ConfirmCodeResult> ProcessConfirmCodeAsync(Guid key);
}

public class SmsUrlAuthenticator : ISmsUrlAuthenticator
{
    readonly IdgwSettings _idgwSettings;
    readonly IConsentSmsService _consentSmsService;
    readonly IBaseMetricsService _siMetricsService;
    readonly ICachedConfigService _cachedConfigService;
    readonly IRedisService _redisService;
    readonly IDbRepository _dbRepository;
    readonly IAuthenticationChainService _authenticationChainService;
    readonly ICachedConfigService _cachedConfig;
    readonly IUserProfileRepository _userProfileRepository;

    static readonly TimeSpan ConfirmationTimeout = TimeSpan.FromMinutes(1);
    static readonly TimeSpan ConfirmCodeLifeTime = ConfirmationTimeout.Add(TimeSpan.FromMinutes(1));

    static readonly UnifiedException CodeExpiredException = new(OAuth2Error.Timeout,
        "Code expired", (int)HttpStatusCode.Conflict);

    static readonly UnifiedException CodeNotExistsException = new(OAuth2Error.InvalidRequest,
        "Code not exist");

    static readonly UnifiedException RequestOrSpNotFoundException = new(OAuth2Error.NotFoundEntity,
        "Request or ServiceProvider for this key not found");

    static readonly UnifiedException AlreadyConfirmedOrRejectedException = new(OAuth2Error.StatusConflict,
        "Payment already confirmed/rejected");

    static readonly UnifiedException PaymentNotAllowed = new(OAuth2Error.StatusConflict, "Payment not allowed");

    string GetRedisUrlStateKey(Guid key) => $"ConfirmService:{key}";

    public SmsUrlAuthenticator(
        IOptions<IdgwSettings> settings,
        IConsentSmsService consentSmsService,
        IBaseMetricsService siMetricsService,
        ICachedConfigService cachedConfigService,
        IRedisService redisService,
        IDbRepository dbRepository,
        IAuthenticationChainService authenticationChainService,
        ICachedConfigService cachedConfig,
        IUserProfileRepository userProfileRepository
    )
    {
        _consentSmsService = consentSmsService;
        _siMetricsService = siMetricsService;
        _cachedConfigService = cachedConfigService;
        _redisService = redisService;
        _dbRepository = dbRepository;
        _authenticationChainService = authenticationChainService;
        _cachedConfig = cachedConfig;
        _userProfileRepository = userProfileRepository;
        _idgwSettings = settings.Value;
    }

    public async Task<AuthenticatorStartResult> TryStartAsync(AuthSessionBag startBag)
    {
        if (startBag.AuthRequest.MinLoA == AcrValues.LoA3)
            return StartResult(AuthenticatorStartResultType.LoA3NotSupported);

        var isPaymentScopeSelected = ScopesHelper.HasPaymentScope(startBag.Scopes);
        var isAuthZScopeSelected = ScopesHelper.HasAuthzScope(startBag.Scopes);

        if (!long.TryParse(startBag.AuthRequest.Msisdn, out var msisdn))
            return StartResult(AuthenticatorStartResultType.MsisdnInvalid);

        var userProfile = await _userProfileRepository.GetUserProfile(msisdn);
        if (userProfile == null) return StartResult(AuthenticatorStartResultType.UserInfoNotFound);
        if (userProfile.IsBlocked) return StartResult(AuthenticatorStartResultType.UserBlocked);

        var serviceProviders = _cachedConfigService.GetState().ServiceProvidersById;
        if (!serviceProviders.TryGetValue(startBag.AuthRequest.ServiceProviderId, out var serviceProvider))
            return StartResult(AuthenticatorStartResultType.ServiceProviderNotFound);

        await CreateCodeAsync(startBag.AuthRequest.ConsentCode, isPaymentScopeSelected);

        var sendResult = await _consentSmsService.SendAsync(
            new ConsentSmsRequest(msisdn,
                serviceProvider.Name!,
                serviceProvider.Scopes,
                startBag.Scopes,
                startBag.AuthRequest.Context,
                startBag.AuthRequest.BindingMessage,
                isAuthZScopeSelected),
            startBag.AuthRequest.ConsentCode,
            isPaymentScopeSelected);

        if (!sendResult) return StartResult(AuthenticatorStartResultType.NotSent);

        _siMetricsService.AddSmsUrlRequest(serviceProvider.Name!);

        return StartResult(AuthenticatorStartResultType.Started);
    }

    static AuthenticatorStartResult StartResult(AuthenticatorStartResultType resultType) => new()
    {
        Authenticator = AuthenticatorType.SmsWithUrl,
        ResultType = resultType,
        StartedLoA = AcrValues.LoA2
    };

    public async Task<Result<PaymentInfoResult>> GetRequestInfoByConfirmCodeAsync(Guid key)
    {
        var redisKey = GetRedisUrlStateKey(key);
        var value = await _redisService.GetStringAsync(redisKey);

        switch (value)
        {
            case null:
                return new Result<PaymentInfoResult>(CodeNotExistsException);
            case RequestConfirmStatus.Expired:
                return new Result<PaymentInfoResult>(CodeExpiredException);
        }

        var authReqDto = await _dbRepository.GetAuthorizationRequestByConsentCodeAsync(key);
        if (authReqDto == null)
            return new Result<PaymentInfoResult>(RequestOrSpNotFoundException);

        Activity.Current?.AddTag(Tags.Msisdn, authReqDto.Msisdn);

        var config = _cachedConfig.GetState();
        if (!config.ServiceProvidersById.TryGetValue(authReqDto.ServiceProviderId, out var serviceProvider))
            return new Result<PaymentInfoResult>(RequestOrSpNotFoundException);

        Activity.Current?.AddTag(Tags.ServiceProvider, serviceProvider.Name);

        var result = new PaymentInfoResult
        {
            ConfirmStatus = value,
            ClientName = serviceProvider.Name,
            OrderSum = authReqDto.OrderSum?.ToString("0.00")
        };
        return result;
    }

    public async Task<ConfirmCodeResult> ProcessConfirmCodeAsync(Guid key)
    {
        var redisKey = GetRedisUrlStateKey(key);

        //сначала проверяем наличие ключа и его статус, чтобы не обновлять время жизни
        var oldValue = await _redisService.GetStringAsync(redisKey);
        if (oldValue != RequestConfirmStatus.WaitingConfirm)
            return new ConfirmCodeResult { IsSuccessful = oldValue == RequestConfirmStatus.Confirmed };

        //выставляем Confirmed и обновляем время жизни, чтобы ссылка была доступна еще некоторое время
        oldValue =
            await _redisService.GetAndSetStringAsync(redisKey, RequestConfirmStatus.Confirmed, ConfirmCodeLifeTime);

        if (oldValue != RequestConfirmStatus.WaitingConfirm)
        {
            await _redisService.SetStringAsync(redisKey, oldValue, ConfirmCodeLifeTime);
            return new ConfirmCodeResult { IsSuccessful = oldValue == RequestConfirmStatus.Confirmed };
        }

        var authReqDto = await _dbRepository.GetAuthorizationRequestByConsentCodeAsync(key);
        if (authReqDto == null) return new ConfirmCodeResult { IsSuccessful = false };

        Activity.Current?.AddTag(Tags.Msisdn, authReqDto.Msisdn);

        var config = _cachedConfig.GetState();
        if (config.ServiceProvidersById.TryGetValue(authReqDto.ServiceProviderId, out var serviceProvider))
            Activity.Current?.AddTag(Tags.ServiceProvider, serviceProvider.Name);

        _ = Task.Run(async () =>
            await _authenticationChainService.TryEndAsync(authReqDto.Msisdn!, AuthenticatorType.SmsWithUrl,
                AuthResult.UserAgree));

        return new ConfirmCodeResult
        {
            IsSuccessful = true,
            AuthorizationRequest = authReqDto,
            ServiceProvider = serviceProvider
        };
    }

    public async Task<Result<bool>> ProcessConfirmCodeAsync(Guid key, ConfirmationReason reason)
    {
        var redisKey = GetRedisUrlStateKey(key);
        //сначала проверяем наличие ключа и его статус, чтобы не обновлять время жизни
        var oldValue = await _redisService.GetStringAsync(redisKey);

        if (oldValue == null)
            return new Result<bool>(CodeNotExistsException);
        if (oldValue != RequestConfirmStatus.WaitingConfirm)
            return new Result<bool>(PaymentNotAllowed);

        //выставляем Confirmed и обновляем время жизни, чтобы ссылка была доступна еще некоторое время
        oldValue = await _redisService.GetAndSetStringAsync(redisKey
            , reason == ConfirmationReason.Confirm ? RequestConfirmStatus.Confirmed : RequestConfirmStatus.Rejected
            , _idgwSettings.SmsUrlPageAvailability);

        if (oldValue != RequestConfirmStatus.WaitingConfirm)
        {
            await _redisService.SetStringAsync(redisKey, oldValue, _idgwSettings.SmsUrlPageAvailability);

            return oldValue == RequestConfirmStatus.Expired
                ? new Result<bool>(CodeExpiredException)
                : new Result<bool>(AlreadyConfirmedOrRejectedException);
        }

        var siAuthReqDto = await _dbRepository.GetAuthorizationRequestByConsentCodeAsync(key);
        if (siAuthReqDto == null)
            return new Result<bool>(RequestOrSpNotFoundException);

        return reason switch
        {
            ConfirmationReason.Confirm =>
                await _authenticationChainService.TryEndAsync(siAuthReqDto.Msisdn!, AuthenticatorType.SmsWithUrl,
                    AuthResult.UserAgree)
                    ? true
                    : new Result<bool>(AlreadyConfirmedOrRejectedException),
            _ => await _authenticationChainService.TryEndAsync(siAuthReqDto.Msisdn!, AuthenticatorType.SmsWithUrl,
                AuthResult.UserDenied)
                ? true
                : new Result<bool>(AlreadyConfirmedOrRejectedException)
        };
    }

    /// <summary>
    /// Создание кода
    /// </summary>
    /// <param name="key"></param>
    /// <param name="isPaymentScopeSelected"></param>
    async Task CreateCodeAsync(Guid key, bool isPaymentScopeSelected)
    {
        await _redisService.SetStringAsync(GetRedisUrlStateKey(key), RequestConfirmStatus.WaitingConfirm,
            isPaymentScopeSelected ? _idgwSettings.SmsUrlPageAvailability : ConfirmCodeLifeTime);
    }
}