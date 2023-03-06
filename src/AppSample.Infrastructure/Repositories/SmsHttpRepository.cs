using AppSample.Domain.DAL;
using AppSample.Domain.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AppSample.CoreTools.Helpers;
using AppSample.CoreTools.Infrustructure.Interfaces;
using AppSample.CoreTools.Infrustructure;
using AppSample.Domain.DAL.DTOs;

namespace AppSample.Infrastructure.Repositories
{
    public class SmsHttpRepository : ISmsHttpRepository
    {
        readonly IdgwSettings _settings;
        readonly ILogger<SIAuthorizeService> _logger;
        readonly IProxyHttpClientFactory _proxyHttpClientFactory;
        readonly IBaseMetricsService _siMetricsService;
        readonly IServiceProviderService _serviceProviderService;
        readonly ITokenService _tokenService;
        readonly IResourceServerService _resourceServerService;

        public SmsHttpRepository(
            IOptions<IdgwSettings> settings,
            ILogger<SIAuthorizeService> logger,
            IProxyHttpClientFactory proxyHttpClientFactory,
            IBaseMetricsService siMetricsService,
            IServiceProviderService serviceProviderService,
            ITokenService tokenService,
            IResourceServerService resourceServerService)
        {
            _serviceProviderService = serviceProviderService;
            _siMetricsService = siMetricsService;
            _settings = settings.Value;
            _logger = logger;
            _proxyHttpClientFactory = proxyHttpClientFactory;
            _tokenService = tokenService;
            _resourceServerService = resourceServerService;
        }

        public string GetSmsOtpEndpoint(Guid smsotpEndpointPathId)
        {
             var smsOtpEndpoint = UrlHelper.Combine(_settings.BasePath, $"/sms_otp/send/{smsotpEndpointPathId}");
            return smsOtpEndpoint;
        }

        public async Task NotifyAboutOtpSmsAsync(AuthorizationRequestDto authRequestDto, string otpNotificationUrl)
        {
            var smsOtpEndpoint = GetSmsOtpEndpoint(authRequestDto.OtpKey);

            SmsOtpNotificationRequest notificationRequest = new SmsOtpNotificationRequest()
            {
                AuthReqId = authRequestDto.AuthorizationRequestId,
                SmsOtpEndpoint = smsOtpEndpoint,
                Send = new SmsOtpNotificationRequestSendInfo() { VerifyCode = "enter_otp_code" },
                CorrelationId = authRequestDto.CorrelationId,
            };

            // Задерживаем отправку запроса, чтобы вызывающая система успела обработать ответ на запрос si-authorize.
            // Так же выполняем несколько попыток отправки запроса до успешного ответа.

            var delays = _settings.OtpFirstNotifyDelays;
            if (delays == null || delays.Length == 0) delays = new[] {TimeSpan.FromSeconds(3)};

            foreach (var delay in delays)
            {
                try
                {
                    if (delay > TimeSpan.Zero) await Task.Delay(delay);
                    var response = await SendNotificationRequest(notificationRequest, otpNotificationUrl, authRequestDto.NotificationToken!);
                    if (response.IsSuccessStatusCode) return;
                }
                catch (Exception exp)
                {
                    _logger.LogError(exp, "Notify call failed");
                    return;
                }
            }
        }
        
        public async Task NotifyAboutSuccessAsync(
            AuthorizationRequestDto entity, AuthenticatorType authenticatorType, ServiceProviderEntity serviceProvider)
        {
            try
            {
                var notificationUri = entity.NotificationUri;

                var tokensCommand = new CreateTokensCommand()
                {
                    Mode = MobileIdMode.SI,
                    ServiceProvider = serviceProvider,
                    Msisdn = entity.Msisdn,
                    AcrValues = entity.AcrValues,
                    Nonce = entity.Nonce,
                    ResponseType = entity.ResponseType,
                    NotificationUri = entity.NotificationUri
                };
                var tokens = await _tokenService.CreateTokensAsync(tokensCommand, authenticatorType);

                NotificationRequestVm notificationRequest = new()
                {
                    AuthorizationRequestId = entity.AuthorizationRequestId,
                    CorrelationId = entity.CorrelationId,
                    AccessToken = tokens.AccessToken,
                    IdToken = tokens.IdToken,
                    ExpiresIn = tokens.ExpiresIn,
                    TokenType = tokens.TokenType
                };

                await _resourceServerService.IntrospectAuthInfoAsync(entity, serviceProvider, tokens);
                await SendNotificationRequest(notificationRequest, notificationUri, entity.NotificationToken);
                
                _siMetricsService.AddDuration(entity.CreatedAt, serviceProvider.Name ?? "<unknown>");
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, "Notify call failed");
            }
        }

        public async Task NotifyAboutFailureAsync(AuthorizationRequestDto authReqDto, NotificationError notificationError)
        {
            NotificationRequestVm notificationRequest = new()
            {
                AuthorizationRequestId = authReqDto.AuthorizationRequestId,
                CorrelationId = authReqDto.CorrelationId,
                Error = notificationError.GetValues().Error,
                ErrorDescription = notificationError.GetValues().Description
            };

            await SendNotificationRequest(notificationRequest, authReqDto.NotificationUri, authReqDto.NotificationToken);
            
            var serviceProvider = await _serviceProviderService.GetByIdAsync(authReqDto.ServiceProviderId);
            _siMetricsService.AddDuration(authReqDto.CreatedAt, serviceProvider.Name ?? "<unknown>");
        }

        async Task<HttpResponseMessage> SendNotificationRequest<TRequest>(
            TRequest notificationRequest,
            string notificationUri,
            string notificationToken) where TRequest : class
        {
            var json = JsonSerializer.Serialize(notificationRequest, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, notificationUri) {Content = jsonContent};
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthorizationSchemes.Bearer, notificationToken);

            var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.DefaultProxy, notificationUri, false);

            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            return httpResponseMessage;
        }
    }
}