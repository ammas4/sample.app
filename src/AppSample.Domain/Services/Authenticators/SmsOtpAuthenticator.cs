using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using AppSample.CoreTools.Redis;
using AppSample.Domain.DAL;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Services.AuthenticationChain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppSample.Domain.Services.Authenticators;

public interface ISmsOtpAuthenticator : IAuthStarter
{
    /// <summary>
    /// Обрабатывает ответ пользователя
    /// </summary>
    /// <param name="authReqId">Это не AuthReqId. Здесь передаётся OtpKey.
    /// Это просто сгенерированный guid по которому хранится сессия Otp и можно достать authRequest</param>
    /// <param name="verifyCode">Код который ввёл пользователь</param>
    /// <returns></returns>
    Task<bool> ProcessOtpCodeAsync(string authReqId, string? verifyCode);
}

public class SmsOtpAuthenticator : ISmsOtpAuthenticator
{
    readonly IRedisService _redisService;
    readonly IDbRepository _dbRepository;
    readonly ILogger<SmsOtpAuthenticator> _logger;
    readonly IdgwSettings _idgwSettings;
    readonly ICachedConfigService _cachedConfigService;
    readonly IConsentSmsService _consentSmsService;
    readonly ISmsHttpRepository _smsHttpRepository;
    readonly IBaseMetricsService _siMetricsService;
    readonly IAuthenticationChainService _authenticationChainService;
    readonly IUserProfileRepository _userProfileRepository;

    public SmsOtpAuthenticator(
        IRedisService redisService,
        IDbRepository dbRepository,
        ILogger<SmsOtpAuthenticator> logger,
        IOptions<IdgwSettings> settings,
        ICachedConfigService cachedConfigService,
        IConsentSmsService consentSmsService,
        ISmsHttpRepository smsHttpRepository,
        IBaseMetricsService siMetricsService,
        IAuthenticationChainService authenticationChainService,
        IUserProfileRepository userProfileRepository
    )
    {
        _redisService = redisService;
        _dbRepository = dbRepository;
        _logger = logger;
        _idgwSettings = settings.Value;
        _cachedConfigService = cachedConfigService;
        _consentSmsService = consentSmsService;
        _smsHttpRepository = smsHttpRepository;
        _siMetricsService = siMetricsService;
        _authenticationChainService = authenticationChainService;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<AuthenticatorStartResult> TryStartAsync(AuthSessionBag startBag)
    {
        if (startBag.AuthRequest.MinLoA == AcrValues.LoA3)
            return StartResult(AuthenticatorStartResultType.LoA3NotSupported);

        if (!long.TryParse(startBag.AuthRequest.Msisdn, out var msisdn))
            return StartResult(AuthenticatorStartResultType.MsisdnInvalid);

        var userProfile = await _userProfileRepository.GetUserProfile(msisdn);
        if (userProfile == null) return StartResult(AuthenticatorStartResultType.UserInfoNotFound);
        if (userProfile.IsBlocked) return StartResult(AuthenticatorStartResultType.UserBlocked);

        var serviceProviders = _cachedConfigService.GetState().ServiceProvidersById;
        if (!serviceProviders.TryGetValue(startBag.AuthRequest.ServiceProviderId, out var serviceProvider))
            return StartResult(AuthenticatorStartResultType.ServiceProviderNotFound);

        if (string.IsNullOrEmpty(serviceProvider.OtpNotifyUrl))
            return StartResult(AuthenticatorStartResultType.OtpNotifyUrlNotFound);

        var isAuthZScopeSelected = ScopesHelper.HasAuthzScope(startBag.Scopes);
        var otpCodeText = await CreateOtpCodeAsync(startBag.AuthRequest.OtpKey);

        var sendResult = await _consentSmsService.SendOtpMessageAsync(
            new ConsentSmsRequest(msisdn,
                serviceProvider.Name!,
                serviceProvider.Scopes,
                startBag.Scopes,
                startBag.AuthRequest.Context,
                startBag.AuthRequest.BindingMessage,
                isAuthZScopeSelected),
            otpCodeText);

        if (!string.IsNullOrEmpty(startBag.SpNotificationToken))
            _ = Task.Run(async () =>
                await _smsHttpRepository.NotifyAboutOtpSmsAsync(startBag.AuthRequest, serviceProvider.OtpNotifyUrl!));

        if (!sendResult) return StartResult(AuthenticatorStartResultType.NotSent);

        _siMetricsService.AddSmsOtpRequest(serviceProvider.Name);
        return StartResult(AuthenticatorStartResultType.Started, serviceProvider.OtpNotifyUrl);
    }

    static AuthenticatorStartResult StartResult(AuthenticatorStartResultType resultType, string? otpNotifyUrl = null) =>
        new()
        {
            Authenticator = AuthenticatorType.SmsOtp,
            ResultType = resultType,
            StartedLoA = AcrValues.LoA2,
            OtpNotifyUrl = otpNotifyUrl
        };

    /// <summary>
    /// Создание кода для OTP и его возврат
    /// </summary>
    /// <param name="key"></param>
    async Task<string> CreateOtpCodeAsync(Guid key)
    {
        var otpKeyLifetime = _idgwSettings.OtpSessionTimeout.Add(TimeSpan.FromMinutes(1));
        //число попыток проверки кода
        await _redisService.SetStringAsync(GetRedisOtpCounterKey(key), _idgwSettings.OtpAttemptsLimit.ToString(),
            otpKeyLifetime);
        //код
        string code;
        if ((_idgwSettings.UseDefaultOtpCode ?? false) && string.IsNullOrEmpty(_idgwSettings.DefaultOtpCode) == false)
        {
            code = _idgwSettings.DefaultOtpCode;
        }
        else
        {
            code = AppSample.CoreTools.Helpers.StringHelper.GeneratePassword(_idgwSettings.OtpCodeLength, "0123456789");
        }

        await _redisService.SetStringAsync(GetRedisOtpCodeKey(key), code, otpKeyLifetime);
        return code;
    }

    public async Task<bool> ProcessOtpCodeAsync(string? authReqId, string? verifyCode)
    {
        if (string.IsNullOrEmpty(authReqId)
            || Guid.TryParse(authReqId, out var authReqIdGuid) == false)
        {
            throw new UnifiedException(OAuth2Error.InvalidRequest, "Mandatory parameter is invalid");
        }

        var counterKey =
            GetRedisOtpCounterKey(authReqIdGuid); //ключ для хранения числа оставшихся попыток проверки кода

        if (await _redisService.IsExistsAsync(counterKey) == false)
        {
            throw new UnifiedException(OAuth2Error.InvalidRequest, "sms session not found") { StatusCode = 404 };
        }

        var remainingAttemptsCount =
            await _redisService.IncrementLongAsync(counterKey, -1, _idgwSettings.OtpSessionTimeout);
        if (remainingAttemptsCount < 0)
        {
            throw new UnifiedException(OAuth2Error.InvalidRequest, "sms session not found") { StatusCode = 404 };
        }

        var codeKey = GetRedisOtpCodeKey(authReqIdGuid); //ключ для хранения кода в SMS
        var code = await _redisService.GetStringAsync(codeKey);

        var authReqDto = await _dbRepository.GetAuthorizationRequestByOtpKeyAsync(authReqIdGuid);
        if (authReqDto == null)
        {
            throw new UnifiedException(OAuth2Error.InvalidRequest, "sms session not found") { StatusCode = 404 };
        }

        if (string.IsNullOrEmpty(verifyCode)
            || verifyCode.Length != _idgwSettings.OtpCodeLength
            || verifyCode.All(x => char.IsAscii(x) && char.IsDigit(x)) == false
            || verifyCode != code)
        {
            if (remainingAttemptsCount > 0)
                throw new UnifiedException(OAuth2Error.InvalidRequest, "invalid otp code")
                    { RetryCount = (int)remainingAttemptsCount };

            if (!await TryDeleteOtpRedisKeysAsync(authReqIdGuid))
                throw new UnifiedException(OAuth2Error.InvalidRequest, "sms session not found") { StatusCode = 404 };

            if (!await _authenticationChainService.TryEndAsync(authReqDto.Msisdn!, AuthenticatorType.SmsOtp,
                    AuthResult.RunOutOfAttempts))
                throw new UnifiedException(OAuth2Error.InvalidRequest, "sms session not found") { StatusCode = 404 };

            throw new UnifiedException(OAuth2Error.InvalidRequest, "invalid otp code")
                { RetryCount = (int)remainingAttemptsCount };
        }
        
        if (!await TryDeleteOtpRedisKeysAsync(authReqIdGuid))
            throw new UnifiedException(OAuth2Error.InvalidRequest, "sms session not found") { StatusCode = 404 };

        if (!await _authenticationChainService.TryEndAsync(authReqDto.Msisdn!, AuthenticatorType.SmsOtp,
                AuthResult.UserAgree))
            throw new UnifiedException(OAuth2Error.InvalidRequest, "sms session not found") { StatusCode = 404 };

        return true;
    }

    async Task<bool> TryDeleteOtpRedisKeysAsync(Guid authorizationRequestId)
    {
        if (await _redisService.DeleteAsync(GetRedisOtpCounterKey(authorizationRequestId)))
        {
            await _redisService.DeleteAsync(GetRedisOtpCodeKey(authorizationRequestId));
            return true;
        }

        return false;
    }

    string GetRedisOtpCounterKey(Guid key) => $"ConfirmService-OtpCounter:{key}";
    string GetRedisOtpCodeKey(Guid key) => $"ConfirmService-OtpCode:{key}";
}