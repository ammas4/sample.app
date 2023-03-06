using System.Net;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Services.AuthenticationChain;
using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using AppSample.CoreTools.Helpers;
using AppSample.CoreTools.Redis;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Models.ServiceProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppSample.Domain.Services.Authenticators;

public interface ISeamlessAuthenticator : IAuthStarter
{
    Task<string> ProcessRequestAsync(Guid interactionId, IPAddress? ipAddress);
    Task<bool> ProcessResultAsync(Guid interactionId, string? xbrToken);
}

public class SeamlessAuthenticator : ISeamlessAuthenticator
{
    const string CreatedValue = "CREATED";
    const string UsedValue = "USED";

    readonly IRedisService _redisService;
    readonly IDbRepository _dbRepository;
    readonly ICachedConfigService _cachedConfigService;
    readonly IAuthenticationChainService _authenticationChainService;
    readonly IBaseMetricsService _siMetricsService;
    readonly IGlobalUrlHelper _globalUrlHelper;
    readonly IdgwSettings _idgwSettings;
    readonly IXbrRepository _xbrRepository;
    readonly IIPRangeService _ipRangeService;
    readonly ILogger<SeamlessAuthenticator> _logger;
    readonly IUserProfileRepository _userProfileRepository;

    public SeamlessAuthenticator(IRedisService redisService,
        IDbRepository dbRepository,
        ICachedConfigService cachedConfigService,
        IAuthenticationChainService authenticationChainService,
        IBaseMetricsService siMetricsService,
        IGlobalUrlHelper globalUrlHelper,
        IOptions<IdgwSettings> idgwSettings,
        IXbrRepository xbrRepository,
        IIPRangeService ipRangeService,
        ILogger<SeamlessAuthenticator> logger,
        IUserProfileRepository userProfileRepository
    )
    {
        _redisService = redisService;
        _dbRepository = dbRepository;
        _cachedConfigService = cachedConfigService;
        _authenticationChainService = authenticationChainService;
        _siMetricsService = siMetricsService;
        _globalUrlHelper = globalUrlHelper;
        _idgwSettings = idgwSettings.Value;
        _xbrRepository = xbrRepository;
        _ipRangeService = ipRangeService;
        _logger = logger;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<AuthenticatorStartResult> TryStartAsync(AuthSessionBag startBag)
    {
        if (startBag.AuthRequest.MinLoA == AcrValues.LoA3)
            return StartResult(AuthenticatorStartResultType.LoA3NotSupported);

        var serviceProviders = _cachedConfigService.GetState().ServiceProvidersById;
        if (!serviceProviders.TryGetValue(startBag.AuthRequest.ServiceProviderId, out var serviceProvider))
            return StartResult(AuthenticatorStartResultType.ServiceProviderNotFound);

        if (!long.TryParse(startBag.AuthRequest.Msisdn, out var msisdn))
            return StartResult(AuthenticatorStartResultType.MsisdnInvalid);

        var userProfile = await _userProfileRepository.GetUserProfile(msisdn);
        if (userProfile == null) return StartResult(AuthenticatorStartResultType.UserInfoNotFound);
        if (userProfile.IsBlocked) return StartResult(AuthenticatorStartResultType.UserBlocked);

        var consentCode = startBag.AuthRequest.ConsentCode;
        await _redisService.SetStringAsync(GetRequestRedisKey(consentCode), CreatedValue, _idgwSettings.HheLinkTimeout);
        var hheUrl = _globalUrlHelper.GetHheRequestUrl(consentCode);

        _siMetricsService.AddSeamlessRequest(serviceProvider.Name!);

        return StartResult(AuthenticatorStartResultType.Started, hheUrl);
    }

    static AuthenticatorStartResult StartResult(AuthenticatorStartResultType resultType, string? hheUrl = null) => new()
    {
        Authenticator = AuthenticatorType.Seamless,
        ResultType = resultType,
        StartedLoA = AcrValues.LoA2,
        HheUrl = hheUrl
    };

    /// <summary>
    /// Обработка запроса /hhe/request 
    /// </summary>
    /// <param name="interactionId"></param>
    /// <param name="ipAddress"></param>
    /// <returns>URL к Header enrichment платформе оператора</returns>
    public async Task<string> ProcessRequestAsync(Guid interactionId, IPAddress? ipAddress)
    {
        //проверяем, что такого тайм-аут запроса ещё не прошел
        var oldValue = await _redisService.GetAndSetStringAsync(GetRequestRedisKey(interactionId),
            UsedValue, _idgwSettings.HheLinkTimeout);
        if (oldValue != CreatedValue)
        {
            //такого interactionId не было, запрос уже обработан, или прошло время жизни hhe-ссылки
            throw new UnifiedException(OAuth2Error.NotFoundEntity);
        }

        if (ipAddress == null || !_ipRangeService.IsBeelineIp(ipAddress))
        {
            var siAuthReqDto = await _dbRepository.GetAuthorizationRequestByConsentCodeAsync(interactionId);
            if (siAuthReqDto != null)
            {
                _ = Task.Run(async () =>
                    await _authenticationChainService.TryEndAsync(siAuthReqDto.Msisdn!, AuthenticatorType.Seamless,
                        AuthResult.UserDenied));
            }

            throw new UnifiedException(OAuth2Error.AuthorizationError);
        }

        //ссылка для возврата
        var redirectUrl = _globalUrlHelper.GetHheEnrichmentUrl(interactionId);
        //URL запроса к Header enrichment платформе оператора
        var hheUrlBuilder = new UrlBuilder(_idgwSettings.XbrUrl);
        hheUrlBuilder.Query["redirect"] = redirectUrl;
        var hheUrl = hheUrlBuilder.ToString();

        await _redisService.SetStringAsync(GetEnrichmentRedisKey(interactionId), CreatedValue,
            _idgwSettings.HheLinkTimeout);

        return hheUrl;
    }

    /// <summary>
    /// Обработка запроса /hhe/enrichment
    /// </summary>
    /// <param name="interactionId"></param>
    /// <param name="xbrToken"></param>
    /// <returns></returns>
    public async Task<bool> ProcessResultAsync(Guid interactionId, string? xbrToken)
    {
        //проверяем, что такого запроса ещё не было и тайм-аут не прошел
        var oldValue = await _redisService.GetAndSetStringAsync(GetEnrichmentRedisKey(interactionId),
            UsedValue, _idgwSettings.HheLinkTimeout);
        if (oldValue != CreatedValue)
        {
            //такого interactionId не было, запрос уже обработан, или прошло время жизни hhe-ссылки
            throw new UnifiedException(OAuth2Error.NotFoundEntity);
        }

        var siAuthReqDto = await _dbRepository.GetAuthorizationRequestByConsentCodeAsync(interactionId);
        if (siAuthReqDto != null)
        {
            //запрос к USSS
            string? xbrMsisdn = await GetMsisdnFromXbrTokenAsync(xbrToken);

            if (!string.IsNullOrEmpty(xbrMsisdn) && siAuthReqDto.Msisdn == xbrMsisdn)
            {
                var config = _cachedConfigService.GetState();
                if (config.ServiceProvidersById.TryGetValue(siAuthReqDto.ServiceProviderId, out var serviceProvider))
                {
                    _ = Task.Run(async () =>
                        await _authenticationChainService.TryEndAsync(siAuthReqDto.Msisdn!, AuthenticatorType.Seamless,
                            AuthResult.UserAgree));
                    return true;
                }
            }
            else
            {
                _ = Task.Run(async () =>
                    await _authenticationChainService.TryEndAsync(siAuthReqDto.Msisdn!, AuthenticatorType.Seamless,
                        AuthResult.UserDenied));
            }
        }

        return false;
    }

    /// <summary>
    /// Получение номера телефона по токену авторизации по Xbr
    /// </summary>
    /// <param name="xbrToken"></param>
    /// <returns></returns>
    async Task<string?> GetMsisdnFromXbrTokenAsync(string? xbrToken)
    {
        if (string.IsNullOrEmpty(xbrToken)) return null;
        try
        {
            return await _xbrRepository.GetMsisdnFromXbrTokenAsync(xbrToken);
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "Usss auth/xbr call failed");
        }

        return null;
    }

    static string GetRequestRedisKey(Guid key) => $"HheService:Request:{key}";
    static string GetEnrichmentRedisKey(Guid key) => $"HheService:Enrichment:{key}";
}