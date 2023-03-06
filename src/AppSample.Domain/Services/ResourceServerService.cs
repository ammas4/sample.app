using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.CoreTools.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using AppSample.CoreTools.Infrastructure;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Helpers;
using AppSample.Domain.Models.ServiceProviders;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace AppSample.Domain.Services;


public class ResourceServerService : IResourceServerService
{
    readonly IProxyHttpClientFactory _proxyHttpClientFactory;
    readonly ResourceServerSettings _settings;
    readonly ILogger<ResourceServerService> _logger;

    public ResourceServerService(IProxyHttpClientFactory proxyHttpClientFactory, IOptions<ResourceServerSettings> settings, ILogger<ResourceServerService> logger)
    {
        _proxyHttpClientFactory = proxyHttpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    /*
    public async Task<StatusInfoResponse> GetStatusInfo(string msisdn, string docType)
    {
        var uri = new Uri(new Uri(_settings.BaseHostAddress!), "/statusinfo").ToString();
        using var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.NoProxy, uri, false);

        var content = JsonSerializer.Serialize(new StatusInfoRequest() {Msisdn = msisdn, DocType = docType});
        using var resourceRequest = new HttpRequestMessage(HttpMethod.Post, uri) {Content = new StringContent(content, Encoding.UTF8, "application/json")};
        var resourceCredentials = _settings.ResourceServer!.DefaultCredentials;
        var basicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{resourceCredentials!.UserName}:{resourceCredentials!.Password}"));
        resourceRequest.Headers.Add(HeaderNames.Authorization, $"Basic {basicAuthValue}");

        var obj = new
        {
            method = resourceRequest.Method,
            uri = uri,
            headers = resourceRequest.Headers,
            body = JsonSerializer.Serialize(content)
        };
        _logger.LogInformation($"Resource server request: {JsonSerializer.Serialize(obj, new JsonSerializerOptions() {Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping})}");

        var resourceResponse = await httpClient.SendAsync(resourceRequest);
        var responseContentString = await resourceResponse.Content.ReadAsStringAsync();


        var obj2 = new
        {
            status = resourceResponse.StatusCode,
            headers = resourceResponse.Headers,
            body = responseContentString,
        };
        _logger.LogInformation($"Resource server response: {JsonSerializer.Serialize(obj2)}");


        return JsonSerializer.Deserialize<StatusInfoResponse>(responseContentString);
    }
    */

    public async Task PrepareWarmingInfoAsync(string msisdn, string scope)
    {
        var includedScopes = StringsHelper.SplitList(scope);
        var uri = new Uri(new Uri(_settings.BaseHostAddress!), _settings.WarmingInfoEndpoint).ToString();
        using var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.IdgwProxy, uri, false);

        //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var content = JsonSerializer.Serialize(new WarmingInfoRequest() { Msisdn = msisdn, Scopes = includedScopes.ToArray()});
        using var resourceRequest = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new StringContent(content, Encoding.UTF8, "application/json") };
        var resourceCredentials = _settings.DefaultCredentials;
        var basicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{resourceCredentials!.UserName}:{resourceCredentials!.Password}"));
        resourceRequest.Headers.Add("Authorization", $"Basic {basicAuthValue}");

        await httpClient.SendAsync(resourceRequest);
    }

    public async Task IntrospectAuthInfoAsync(AuthorizationRequestDto entity, ServiceProviderEntity serviceProvider, CreateTokensResult tokens)
    {
        var authorizeScopesNames = entity.Scope!.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (authorizeScopesNames.Any(x => ScopesHelper.IsSpecialScope(x)) == false) return;

        var token = new ResourceServerAuthToken()
        {
            AccessToken = tokens.AccessToken,
            IdToken = tokens.IdToken,
            ExpiresIn = tokens.ExpiresIn,
            TokenType = "Bearer"
        };

        var selectedScopes = serviceProvider.Scopes?.Where(s => ScopesHelper.IsPremiumScope(s.ScopeName) && authorizeScopesNames.Contains(s.ScopeName)).ToArray();
        var hasDocTypes = serviceProvider.DocTypes?.Any() ?? false;
        var resourceServerAuthPostRequest = new ResourceServerAuthPostRequest()
        {
            Jwks = serviceProvider.JwksEncContent,
            JwksUrl = serviceProvider.JwksEncUrl,
            Msisdn = entity.Msisdn,
            Scopes = selectedScopes,
            Token = token,
            EncrAlg = serviceProvider.EncryptionAlgorithm.GetDescription(),
            EncrMethod = serviceProvider.EncryptionMethod.GetDescription(),
            DocTypes = hasDocTypes ? serviceProvider?.DocTypes?.ConvertAll(docType => (byte)docType) : null,
        };

        try
        {
            await IntrospectAuthInfoAsync(resourceServerAuthPostRequest);
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "Auth info call failed");
        }
    }

    async Task IntrospectAuthInfoAsync(ResourceServerAuthPostRequest resourceServerAuthPostRequest)
    {
        var uri = new Uri(new Uri(_settings.BaseHostAddress!), _settings.AuthInfoEndpoint).ToString();
        using var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.IdgwProxy, uri, false);

        var content = JsonSerializer.Serialize(resourceServerAuthPostRequest);
        using var resourceRequest = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new StringContent(content, Encoding.UTF8, "application/json") };
        Credentials resourceCredentials = _settings.DefaultCredentials;
        var basicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{resourceCredentials!.UserName}:{resourceCredentials!.Password}"));
        resourceRequest.Headers.Add("Authorization", $"Basic {basicAuthValue}");

        await httpClient.SendAsync(resourceRequest);
    }

}


