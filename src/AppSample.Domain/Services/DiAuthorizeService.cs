using System.Collections.Immutable;
using System.Diagnostics;
using AppSample.CoreTools.Helpers;
using AppSample.Domain.DAL;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.Constants;
using AppSample.Domain.Models.ServiceProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using AppSample.Domain.Extensions.Models;
using AppSample.Domain.Services.AuthenticationChain;

namespace AppSample.Domain.Services;

public class DiAuthorizeService : IDiAuthorizeService
{
    readonly ICachedConfigService _cachedConfigService;
    readonly IDbRepository _dbRepository;
    readonly ITokenService _tokenService;
    readonly ISmsHttpRepository _smsOtpRepository;
    readonly IDiAuthStateService _diAuthStateService;
    readonly IdgwSettings _settings;
    readonly ILogger<SIAuthorizeService> _logger;
    readonly IAuthenticationChainService _authenticationChainService;
    readonly IResourceServerService _resourceServerService;

    public DiAuthorizeService(ICachedConfigService cachedConfigService,
        IDbRepository dbRepository,
        ITokenService tokenService,
        ISmsHttpRepository smsOtpRepository,
        IDiAuthStateService diAuthStateService,
        IOptions<IdgwSettings> settings,
        ILogger<SIAuthorizeService> logger, IAuthenticationChainService authenticationChainService,
        IResourceServerService resourceServerService)
    {
        _cachedConfigService = cachedConfigService;
        _dbRepository = dbRepository;
        _tokenService = tokenService;
        _smsOtpRepository = smsOtpRepository;
        _diAuthStateService = diAuthStateService;
        _settings = settings.Value;
        _logger = logger;
        _authenticationChainService = authenticationChainService;
        _resourceServerService = resourceServerService;
    }

    public async Task<DiAuthorizationResult> DiAuthorizeGenAsync(DiAuthorizationCommand command)
    {
        var serviceProvider = GetServiceProvider(command.ClientId);

        Activity.Current?.AddTag(Tags.ServiceProvider, serviceProvider.Name);

        ValidateWithoutRedirect(command, serviceProvider);

        try
        {
            var validationResult = ValidateWithRedirect(command);

            var msisdnStr = GetMsisdn(command.LoginHint);
            var msisdn = long.Parse(msisdnStr);

            if (command.HasSpecialScope())
            {
                // интеграция с RS /warminginfo для прогрева ПДн
                WarmPremiumInfo(msisdn, command.Scope);
            }

            var authRequest = await CreateAuthRequestAsync(serviceProvider.Id, msisdnStr, command);

            await _diAuthStateService.InitAsync(authRequest.AuthorizationRequestId);
            _ = _authenticationChainService.StartAsync(authRequest, validationResult.Scopes, null);

            var verificationUrl = _smsOtpRepository.GetSmsOtpEndpoint(authRequest.OtpKey);
            var result = new DiAuthorizationResult
            {
                RedirectUrl = command.ConfirmationUrlBuilder(serviceProvider.Type.GetAuthMode(), msisdnStr, verificationUrl),
                SessionId = authRequest.AuthorizationRequestId.ToString(),
                SpSiteUrl = (string.IsNullOrEmpty(serviceProvider.AuthPageUrl) == false ? serviceProvider.AuthPageUrl : command.RedirectUri) ?? "",
                SpSiteLabel = serviceProvider.Name ?? "",
                OtpKey = authRequest.OtpKey.ToString(),
            };
            return result;
        }
        catch (UnifiedException exp)
        {
            _logger.LogError(exp, "Failed: DiAuthorize");
            var result = new DiAuthorizationResult
            {
                RedirectUrl =
                    GetRedirectUriWithError(command, OAuth2ErrorDetails.GetText(exp.Error), exp.ErrorDescription)
            };
            return result;
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "Failed: DiAuthorize. Server Error");
            var result = new DiAuthorizationResult
            {
                RedirectUrl = GetRedirectUriWithError(command, "server_error", null)
            };
            return result;
        }
    }

    void WarmPremiumInfo(long msisdn, string? scope)
    {
        //TODO
        Task.Run(async () => await _resourceServerService.PrepareWarmingInfoAsync(msisdn.ToString(), scope!));
    }

    DiRequestValidationInfo ValidateWithRedirect(DiAuthorizationCommand request)
    {
        if (string.IsNullOrEmpty(request.Scope))
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.Scope));
        if (string.IsNullOrEmpty(request.ResponseType))
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.ResponseType));
        if (string.IsNullOrEmpty(request.Nonce))
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.Nonce));
        if (string.IsNullOrEmpty(request.Version))
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(MobileConnectParameterNames.Version));
        if (string.IsNullOrEmpty(request.State))
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.State));

        var scopes = request.Scope!.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToImmutableHashSet();

        if (scopes.Distinct().Count() < 2 || !scopes.Contains("openid"))
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.Scope));
        if (request.ResponseType != "code")
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.ResponseType));

        try
        {
            var acrValues = request.AcrValues!.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)
                .ToArray();
            var acrValuesAllowed = new int[] { 2, 3, 4 };
            if (acrValues.Length == 0 || acrValues.Intersect(acrValuesAllowed).Count() != acrValues.Length)
                throw new UnifiedException(OAuth2Error.InvalidRequest,
                    GetInvalidDescription(OpenIdConnectParameterNames.AcrValues));
        }
        catch
        {
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.AcrValues));
        }

        return new DiRequestValidationInfo
        {
            Scopes = scopes
        };
    }

    string GetRedirectUriWithError(DiAuthorizationCommand command, string error, string? errorDescription)
    {
        var urlBuilder = new UrlBuilder(command.RedirectUri!);
        urlBuilder.Query["state"] = command.State;
        urlBuilder.Query["error"] = error;
        if (!string.IsNullOrEmpty(errorDescription))
        {
            urlBuilder.Query["error_description"] = errorDescription;
        }

        if (!string.IsNullOrEmpty(command.CorrelationId))
        {
            urlBuilder.Query["correlation_id"] = command.CorrelationId;
        }

        var redirectUrl = urlBuilder.ToString();
        return redirectUrl;
    }

    string? GetMsisdn(string? loginHint)
    {
        if (string.IsNullOrEmpty(loginHint))
        {
            return null;
        }

        if (LoginHintHelper.ContainsEncrMsisdn(loginHint))
        {
            // функционал пока не поддерживается	
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.LoginHint));
        }

        if (LoginHintHelper.ContainsMsisdn(loginHint))
        {
            var msisdn = LoginHintHelper.GetValueWithoutPrefix(loginHint);

            if (Helpers.MsisdnHelper.IsValid(msisdn) == false)
                throw new UnifiedException(OAuth2Error.InvalidRequest,
                    GetInvalidDescription(OpenIdConnectParameterNames.LoginHint));

            return msisdn;
        }
        else if (LoginHintHelper.ContainsPcr(loginHint))
        {
            // функционал пока не поддерживается	
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.LoginHint));
        }
        else
        {
            throw new UnifiedException(OAuth2Error.InvalidRequest,
                GetInvalidDescription(OpenIdConnectParameterNames.LoginHint));
        }
    }

    async Task<AuthorizationRequestDto> CreateAuthRequestAsync(int serviceProviderId, string? msisdn,
        DiAuthorizationCommand command)
    {
        var authReqDto = new AuthorizationRequestDto
        {
            AuthorizationRequestId = Guid.NewGuid(),
            RedirectUri = command.RedirectUri,
            State = command.State,
            ServiceProviderId = serviceProviderId,
            Msisdn = msisdn,
            Scope = command.Scope,
            AcrValues = command.AcrValues,
            ResponseType = command.ResponseType,
            ConsentCode = Guid.NewGuid(),
            OtpKey = Guid.NewGuid(),
            Nonce = command.Nonce,
            CorrelationId = command.CorrelationId,
            CreatedAt = DateTime.UtcNow,
            Context = command.Context,
            BindingMessage = command.BindingMessage,
            Mode = MobileIdMode.DI,
        };

        await _dbRepository.InsertAuthorizationRequestAsync(authReqDto);

        return authReqDto;
    }

    ServiceProviderEntity GetServiceProvider(string? clientId)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new UnifiedException(OAuth2Error.UnauthorizedClient,
                GetInvalidDescription(OpenIdConnectParameterNames.ClientId));

        var configState = _cachedConfigService.GetState();
        configState.ServiceProvidersByClientId.TryGetValue(clientId, out var serviceProvider);

        if (serviceProvider == null)
            throw new UnifiedException(OAuth2Error.UnauthorizedClient,
                GetInvalidDescription(OpenIdConnectParameterNames.ClientId));

        if (serviceProvider.Active == false || serviceProvider.Deleted)
            throw new UnifiedException(OAuth2Error.AccessDenied,
                "The client is not allowed to make MC service requests.");

        return serviceProvider;
    }

    public async Task<DiCheckConfirmationResult> CheckConfirmationAsync(string sessionIdStr)
    {
        if (Guid.TryParse(sessionIdStr, out Guid sessionId) == false)
        {
            throw new UnifiedException(OAuth2Error.UnauthorizedClient, GetInvalidDescription("session_id"));
        }

        var statusInfo = await _diAuthStateService.GetStatusAsync(sessionId);

        AuthorizationRequestDto? authRequestDto = null;
        if (statusInfo.Status == CheckConfirmationStatus.OK)
        {
            authRequestDto = await _dbRepository.GetAuthorizationRequestAsync(sessionId);
            if (authRequestDto == null)
            {
                statusInfo = new DiStatusInfo()
                {
                    Status = CheckConfirmationStatus.Timeout
                };
            }
        }

        switch (statusInfo.Status)
        {
            case CheckConfirmationStatus.OK:
                return new DiCheckConfirmationResult()
                {
                    Status = statusInfo.Status,
                    RequestRedirectUrl = authRequestDto!.RedirectUri,
                    Code = statusInfo.Code,
                    State = authRequestDto.State,
                    CorrelationId = authRequestDto.CorrelationId
                };

            case CheckConfirmationStatus.Wait:
                return new DiCheckConfirmationResult()
                {
                    Status = statusInfo.Status,
                    TimerRemaining = statusInfo.TimerRemaining
                };

            default:
                return new DiCheckConfirmationResult()
                {
                    Status = statusInfo.Status
                };
        }
    }

    /// <summary>
    /// Создание токенов по коду ответа на /authorize
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="UnifiedException"></exception>
    public async Task<McTokenResult> McTokenAsync(McTokenCommand command)
    {
        if (command.AuthInfo == null)
            throw new UnifiedException(OAuth2Error.UnauthorizedClient, "Authorization headers not found");

        var config = _cachedConfigService.GetState();
        if (config.ServiceProvidersByClientId.TryGetValue(command.AuthInfo.Login!, out var serviceProvider) == false
            || serviceProvider.Active == false
            || serviceProvider.Deleted
            || serviceProvider.ClientSecret != command.AuthInfo.Password)
            throw new UnifiedException(OAuth2Error.UnauthorizedClient, "Client Id or Client Secret is invalid");

        if (command.GrantType != MobileConnectGrantTypes.AuthorizationCode)
            throw new UnifiedException(OAuth2Error.InvalidGrant, "REQUIRED parameter grant_type is invalid");

        if (string.IsNullOrEmpty(command.Code))
            throw new UnifiedException(OAuth2Error.InvalidRequest, "REQUIRED parameter code is empty");

        var (authId, authenticatorType) = await _diAuthStateService.GetAuthIdByOneTimeCodeAsync(command.Code);
        if (authId == null)
            throw new UnifiedException(OAuth2Error.InvalidRequest, "Unable to create access_token by request");

        var entity = await _dbRepository.GetAuthorizationRequestAsync(authId.Value);
        if (entity == null)
            throw new UnifiedException(OAuth2Error.InvalidRequest, "Unable to create access_token by request");

        if (entity.ServiceProviderId != serviceProvider.Id)
            throw new UnifiedException(OAuth2Error.InvalidGrant, "REQUIRED parameter code is invalid");

        if (entity.RedirectUri != command.RedirectUri)
            throw new UnifiedException(OAuth2Error.InvalidRequest, "REQUIRED parameter redirect_uri is invalid");

        var tokensCommand = new CreateTokensCommand()
        {
            Mode = MobileIdMode.DI,
            AcrValues = entity.AcrValues,
            Nonce = entity.Nonce,
            ResponseType = entity.ResponseType,
            NotificationUri = null,
            Msisdn = entity.Msisdn,
            ServiceProvider = serviceProvider,
        };
        var tokens = await _tokenService.CreateTokensAsync(tokensCommand, authenticatorType);

        await _resourceServerService.IntrospectAuthInfoAsync(entity, serviceProvider, tokens);

        McTokenResult result = new McTokenResult()
        {
            AccessToken = tokens.AccessToken,
            CorrelationId = entity.CorrelationId,
            ExpiresIn = tokens.ExpiresIn,
            IdToken = tokens.IdToken,
            Scope = entity.Scope,
            TokenType = tokens.TokenType,
        };

        return result;
    }

    void ValidateWithoutRedirect(DiAuthorizationCommand command, ServiceProviderEntity serviceProvider)
    {
        if (string.IsNullOrEmpty(command.ClientId))
            throw new UnifiedException(OAuth2Error.UnauthorizedClient,
                GetMissingDescription(OpenIdConnectParameterNames.ClientId));

        if (Uri.TryCreate(command.RedirectUri, UriKind.Absolute, out var uri) == false)
            throw new UnifiedException(OAuth2Error.UnauthorizedClient,
                GetMissingOrInvalidDescription(OpenIdConnectParameterNames.RedirectUri));

        if (string.IsNullOrEmpty(command.RedirectUri)
            || serviceProvider.RedirectUrls.Count == 0
            || serviceProvider.RedirectUrls!.All(x => x != command.RedirectUri))
        {
            throw new UnifiedException(OAuth2Error.UnauthorizedClient,
                GetMissingOrInvalidDescription(OpenIdConnectParameterNames.RedirectUri));
        }
    }

    string GetInvalidDescription(string parameterName) => $"Mandatory parameter {parameterName} is invalid";

    string GetMissingOrInvalidDescription(string parameterName) =>
        $"Mandatory parameter {parameterName} is missing or invalid";

    string GetMissingDescription(string parameterName) => $"Mandatory parameter {parameterName} is missing";
}