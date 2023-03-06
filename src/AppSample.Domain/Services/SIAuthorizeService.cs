using System.Collections.Immutable;
using AppSample.Domain.DAL;
using AppSample.Domain.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.CoreTools.Redis;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using AppSample.Domain.Models.Constants;
using System.Diagnostics;
using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using AppSample.CoreTools.Infrustructure.Interfaces;
using AppSample.CoreTools.Infrustructure;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Services.AuthenticationChain;


namespace AppSample.Domain.Services;

public class SIAuthorizeService : ISIAuthorizeService
{
    static readonly string[] RequiredJwtParamters = new[]
    {
        OpenIdConnectParameterNames.AcrValues,
        OpenIdConnectParameterNames.ClientId,
        OpenIdConnectParameterNames.Iss,
        OpenIdConnectParameterNames.LoginHint,
        OpenIdConnectParameterNames.Nonce,
        OpenIdConnectParameterNames.Scope,
        MobileConnectParameterNames.ClientNotificationToken,
        MobileConnectParameterNames.NotificationUri,
        MobileConnectParameterNames.Version,
        // Агрегатор сейчас отправляет пустое значение в этом параметре
        // JwtRegisteredClaimNames.Aud, 
    };

    readonly IDbRepository _dbRepository;
    readonly ICachedConfigService _cachedConfigService;
    readonly IdgwSettings _settings;
    readonly IProxyHttpClientFactory _proxyHttpClientFactory;
    readonly IRedisService _redisService;
    readonly CacheSettings _cacheSettings;
    readonly IBaseMetricsService _siMetricsService;
    readonly IResourceServerService _resourceServerService;
    readonly IAuthenticationChainService _authenticationChainService;

    public SIAuthorizeService(
        IDbRepository dbRepository,
        ICachedConfigService cachedConfigService,
        IOptions<IdgwSettings> settings,
        IProxyHttpClientFactory proxyHttpClientFactory,
        IRedisService redisService,
        IResourceServerService resourceServerService,
        IBaseMetricsService siMetricsService,
        IOptions<CacheSettings> cacheSettings,
        IAuthenticationChainService authenticationChainService)
    {
        _dbRepository = dbRepository;
        _cachedConfigService = cachedConfigService;
        _settings = settings.Value;
        _proxyHttpClientFactory = proxyHttpClientFactory;
        _redisService = redisService;
        _siMetricsService = siMetricsService;
        _resourceServerService = resourceServerService;
        _authenticationChainService = authenticationChainService;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<SiAuthorizationResult> SiAuthorizeGen(string? clientId, string? scope, string? request,
        string? responseType)
    {
        var serviceProvider = await AuthorizeAndGetServiceProviderAsync(clientId!, request!);

        var validationResult = ValidateAndGetJwtPayload(serviceProvider, clientId, scope, request, responseType);

        var msisdn = long.Parse(validationResult.Msisdn);

        Activity.Current?.AddTag(Tags.Msisdn, msisdn);

        if (validationResult.Scopes.Any(x => ScopesHelper.IsSpecialScope(x)))
        {
            // интеграция с RS /warminginfo для прогрева ПДн
            WarmPremiumInfo(msisdn, scope);
        }

        // Если response_type не указан в параметрах запроса, пробуем взять его из payload
        // Если response_type не указан нигде, по дефолту используем mc_si_async_code
        responseType ??= validationResult.ResponseType ?? MobileConnectResponseTypes.SIAsyncCode;

        _siMetricsService.AddRequest(serviceProvider.Name);
        Activity.Current?.AddTag(Tags.ServiceProvider, serviceProvider.Name);

        var authRequest = await CreateAuthRequestAsync(serviceProvider.Id, validationResult, scope!, responseType);
        var chainStartResult = await _authenticationChainService.StartAsync(authRequest, validationResult.Scopes,
            validationResult.SPNotificationToken);

        if (!chainStartResult.First.HasValue)
            throw new UnifiedException(OAuth2Error.AuthorizationError,
                GetErrorDescription(AuthenticatorStartResultType.NoValue));

        if (!chainStartResult.FirstStarted.HasValue)
            throw new UnifiedException(OAuth2Error.AuthorizationError,
                GetErrorDescription(chainStartResult.First.Value.ResultType));

        return new SiAuthorizationResult(
            authRequest.AuthorizationRequestId,
            _settings.AuthorizationRequestExpiresInSec,
            authRequest.CorrelationId,
            null, null, null,
            chainStartResult.FirstStarted.Value.HheUrl);
    }

    static string GetErrorDescription(AuthenticatorStartResultType type) => type switch
    {
        AuthenticatorStartResultType.UserBlocked => "Subscriber blocked",
        AuthenticatorStartResultType.SimCardNotSupport =>
            "Subscriber SIM card does not support authentication request",
        AuthenticatorStartResultType.LoA3NotSupported =>
            "Subscriber SIM card does not support authentication request",
        _ => "Auth request end with error"
    };

    void WarmPremiumInfo(long msisdn, string? scope)
    {
        //TODO
        Task.Run(async () => await _resourceServerService.PrepareWarmingInfoAsync(msisdn.ToString(), scope!));
    }

    async Task<AuthorizationRequestDto> CreateAuthRequestAsync(
        int serviceProviderId,
        SiRequestValidationInfo validationResult,
        string scope, string responseType)
    {
        var siAuthReqDto = new AuthorizationRequestDto
        {
            NotificationToken = validationResult.SPNotificationToken,
            NotificationUri = validationResult.SPNotificationUri,
            ServiceProviderId = serviceProviderId,
            Msisdn = validationResult.Msisdn,
            Scope = scope,
            AcrValues = validationResult.AcrValues,
            ResponseType = responseType,
            AuthorizationRequestId = Guid.NewGuid(),
            ConsentCode = Guid.NewGuid(),
            OtpKey = Guid.NewGuid(),
            Nonce = validationResult.Nonce,
            CorrelationId = validationResult.CorrelationId,
            CreatedAt = DateTime.UtcNow,
            OrderSum = validationResult.OrderSum,
            Context = validationResult.Context,
            BindingMessage = validationResult.BindingMessage,
            Mode = MobileIdMode.SI,
        };

        await _dbRepository.InsertAuthorizationRequestAsync(siAuthReqDto);

        return siAuthReqDto;
    }

    async Task<ServiceProviderEntity> AuthorizeAndGetServiceProviderAsync(string clientId, string request)
    {
        var configState = _cachedConfigService.GetState();
        configState.ServiceProvidersByClientId.TryGetValue(clientId, out var serviceProvider);

        if (serviceProvider == null)
            throw new UnifiedException(OAuth2Error.UnauthorizedClient,
                GetInvalidDescription(OpenIdConnectParameterNames.ClientId));

        if (serviceProvider.Active == false || serviceProvider.Deleted)
            throw new UnifiedException(OAuth2Error.AccessDenied,
                "The client is not allowed to make MC service requests.");

        var jwksString = await GetServiceProviderJwks(serviceProvider);

        if (string.IsNullOrEmpty(jwksString) || !JwtSignatureValidator.Validate(request, jwksString!, out var claims))
            throw new UnifiedException(OAuth2Error.UnauthorizedClient, "Token validation failed");

        claims!.TryGetValue(MobileConnectParameterNames.NotificationUri, out var temp);
        var notificationUri = (string?)temp;

        if (string.IsNullOrEmpty(notificationUri)
            || serviceProvider.NotificationUrls.Count == 0
            || serviceProvider.NotificationUrls!.All(x => x != notificationUri))
        {
            throw new UnifiedException(OAuth2Error.UnauthorizedClient,
                GetInvalidDescription(MobileConnectParameterNames.Request));
        }

        return serviceProvider;
    }

    SiRequestValidationInfo ValidateAndGetJwtPayload(
        ServiceProviderEntity serviceProvider, string? clientId, string? scope, string? request, string? responseType)
    {
        foreach (var parameter in new (string Name, string? Value)[]
                 {
                     (OpenIdConnectParameterNames.ClientId, clientId),
                     (OpenIdConnectParameterNames.Scope, scope),
                     (MobileConnectParameterNames.Request, request)
                 })
        {
            if (parameter.Value == null)
                throw new UnifiedException(OAuth2Error.InvalidRequest,
                    $"Mandatory parameter {parameter.Name} is missing");
            if (string.IsNullOrWhiteSpace(parameter.Value))
                throw new UnifiedException(OAuth2Error.InvalidRequest, GetInvalidDescription(parameter.Name));
        }

        if (responseType != null && !MobileConnectResponseTypes.ValidResponseTypes.Contains(responseType))
        {
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                $"Parameter {OpenIdConnectParameterNames.ResponseType} is invalid");
        }

        var scopes = scope!.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToImmutableHashSet();
        if (scopes.Count < 2 || !scopes.Contains(OpenIdConnectScope.OpenId))
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.Scope));

        var claims = new JsonWebToken(request).Claims.ToImmutableDictionary(c => c.Type, c => c.Value);

        ValidateRequiredJwtParamters(claims);

        ValidateClientId(claims, clientId!);
        ValidateClientName(claims, serviceProvider, isRequired: scopes.Contains(ScopesHelper.AuthzScope));
        ValidateVersion(claims);
        ValidateScopes(claims, scopes);
        ValidateIssuer(claims, serviceProvider);

        var acrValues = ValidateAndGetAcrValues(claims);
        var msisdn = ValidateLoginHintAndGetMsisdn(claims);
        var responseTypeFromPayload = ValidateAndGetResponseType(claims, responseType);
        var orderSum = ValidateAndGetOrderSum(claims, scopes);

        return new SiRequestValidationInfo()
        {
            Msisdn = msisdn,
            AcrValues = acrValues,
            SpHitTs = claims.GetValueOrDefault(MobileConnectParameterNames.SpHitTs),
            ResponseType = responseTypeFromPayload,
            Scopes = scopes,
            SPNotificationUri = claims[MobileConnectParameterNames.NotificationUri],
            Nonce = claims.GetValueOrDefault(OpenIdConnectParameterNames.Nonce),
            CorrelationId = claims.GetValueOrDefault(MobileConnectParameterNames.CorrelationId),
            SPNotificationToken = claims[MobileConnectParameterNames.ClientNotificationToken],
            Context = claims.GetValueOrDefault(MobileConnectParameterNames.Context),
            OrderSum = orderSum,
            BindingMessage = claims.GetValueOrDefault(MobileConnectParameterNames.BindingMessage),
        };
    }

    async Task<string?> GetServiceProviderJwks(ServiceProviderEntity serviceProvider)
    {
        if (!string.IsNullOrEmpty(serviceProvider.JwksContent))
        {
            return serviceProvider.JwksContent;
        }

        if (!string.IsNullOrEmpty(serviceProvider.JwksUrl))
        {
            var key = CacheKeys.ServiceProviderJwks(serviceProvider.Id);

            var jwks = await _redisService.GetStringAsync(key);

            if (!string.IsNullOrEmpty(jwks))
            {
                return jwks;
            }

            var httpClient = _proxyHttpClientFactory.CreateHttpClient(
                NamedHttpClient.DefaultProxy, serviceProvider.JwksUrl, allowUntrustedSsl: false);

            jwks = await httpClient.GetStringAsync(serviceProvider.JwksUrl);

            if (!string.IsNullOrEmpty(jwks))
            {
                await _redisService.SetStringAsync(key, jwks, _cacheSettings.ServiceProdiverJwksCacheTime,
                    StackExchange.Redis.CommandFlags.FireAndForget);
            }

            return jwks;
        }

        return null;
    }

    static void ValidateRequiredJwtParamters(ImmutableDictionary<string, string> claims)
    {
        foreach (var parameterName in RequiredJwtParamters)
        {
            if (!claims.ContainsKey(parameterName))
            {
                throw MissingJwtParameterException(parameterName);
            }

            var parameterValue = claims[parameterName];

            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                throw InvalidJwtParameterException(parameterName);
            }
        }
    }

    static void ValidateClientId(ImmutableDictionary<string, string> claims, string clientIdFromParameters)
    {
        var clientIdFromPayload = claims[OpenIdConnectParameterNames.ClientId];

        if (clientIdFromPayload != clientIdFromParameters)
        {
            throw InvalidJwtParameterException(OpenIdConnectParameterNames.ClientId);
        }
    }

    static string ValidateAndGetAcrValues(ImmutableDictionary<string, string> claims)
    {
        var acrValues = claims[OpenIdConnectParameterNames.AcrValues];

        var firstValidAcrValue = AcrValuesHelper.GetFirstSupportedValue(claims[OpenIdConnectParameterNames.AcrValues]);

        if (firstValidAcrValue == null)
        {
            throw InvalidJwtParameterException(OpenIdConnectParameterNames.AcrValues);
        }

        return acrValues;
    }

    static decimal? ValidateAndGetOrderSum(
        ImmutableDictionary<string, string> claims,
        ImmutableHashSet<string> scopes)
    {
        if (ScopesHelper.HasPaymentScope(scopes))
        {
            var orderSumFromClaim = claims.GetValueOrDefault(MobileConnectParameterNames.OrderSum);

            if (orderSumFromClaim == null)
                throw MissingJwtParameterException(MobileConnectParameterNames.OrderSum);

            var orderSum = PaymentHelper.ParseOrderSumString(orderSumFromClaim);

            if (orderSum == null)
                throw InvalidJwtParameterException(MobileConnectParameterNames.OrderSum);

            return orderSum;
        }

        return null;
    }

    static void ValidateClientName(
        ImmutableDictionary<string, string> claims,
        ServiceProviderEntity serviceProvider,
        bool isRequired)
    {
        var clientName = claims.GetValueOrDefault(MobileConnectParameterNames.ClientName);

        if (clientName == null && isRequired)
        {
            throw MissingJwtParameterException(MobileConnectParameterNames.ClientName);
        }

        if (clientName != null && serviceProvider.Name != clientName)
        {
            throw InvalidJwtParameterException(MobileConnectParameterNames.ClientName);
        }
    }

    static void ValidateVersion(ImmutableDictionary<string, string> claims)
    {
        var version = claims[MobileConnectParameterNames.Version];

        if (!VersionHelper.IsSupportedSIVersion(version))
        {
            throw InvalidJwtParameterException(MobileConnectParameterNames.Version);
        }
    }

    static void ValidateScopes(
        ImmutableDictionary<string, string> claims,
        ImmutableHashSet<string> scopesFromParameters)
    {
        var scopesFromPayload = claims[OpenIdConnectParameterNames.Scope]
            .Split(" ", StringSplitOptions.RemoveEmptyEntries).ToImmutableHashSet();

        if (!scopesFromParameters.SequenceEqual(scopesFromPayload))
        {
            throw InvalidJwtParameterException(OpenIdConnectParameterNames.Scope);
        }
    }

    void ValidateIssuer(
        ImmutableDictionary<string, string> claims,
        ServiceProviderEntity serviceProvider)
    {
        var issuer = claims[OpenIdConnectParameterNames.Iss];

        if (issuer != serviceProvider.ClientId && !_settings.AllowedRequestTokenIssuers.Contains(issuer))
        {
            throw InvalidJwtParameterException(OpenIdConnectParameterNames.Iss);
        }
    }

    static string ValidateLoginHintAndGetMsisdn(ImmutableDictionary<string, string> claims)
    {
        var loginHint = claims[OpenIdConnectParameterNames.LoginHint];

        if (loginHint.StartsWith("MSISDN:"))
        {
            var msisdn = loginHint.Split(":", StringSplitOptions.RemoveEmptyEntries).Last();

            if (MsisdnHelper.IsValid(msisdn))
            {
                return msisdn;
            }
        }

        throw InvalidJwtParameterException(OpenIdConnectParameterNames.LoginHint);
    }

    static string? ValidateAndGetResponseType(
        ImmutableDictionary<string, string> claims,
        string? responseTypeFromParameters)
    {
        var responseTypeFromPayload = claims.GetValueOrDefault(OpenIdConnectParameterNames.ResponseType);

        if (responseTypeFromPayload != null)
        {
            var isValidResponseType = MobileConnectResponseTypes.ValidResponseTypes.Contains(responseTypeFromPayload);

            if (!isValidResponseType
                || (responseTypeFromParameters != null && responseTypeFromPayload != responseTypeFromParameters))
            {
                throw InvalidJwtParameterException(OpenIdConnectParameterNames.ResponseType);
            }
        }

        return responseTypeFromPayload;
    }

    static UnifiedException MissingJwtParameterException(string parameterName) =>
        new(OAuth2Error.InvalidRequest, GetJwtParameterMissingDescription(parameterName));

    static UnifiedException InvalidJwtParameterException(string parameterName) =>
        new(OAuth2Error.InvalidRequest, GetJwtParameterIvalidDescription(parameterName));

    string GetInvalidDescription(string parameterName) => $"Mandatory parameter {parameterName} is invalid";

    static string GetJwtParameterMissingDescription(string parameterName)
        => $"REQUIRED parameter {parameterName} is missing";

    static string GetJwtParameterIvalidDescription(string parameterName)
        => $"REQUIRED parameter {parameterName} is invalid";
}