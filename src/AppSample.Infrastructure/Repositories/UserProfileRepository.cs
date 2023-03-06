using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppSample.CoreTools.Extensions;
using AppSample.CoreTools.Infrustructure;
using AppSample.CoreTools.Infrustructure.Interfaces;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.Ups;
using AppSample.Domain.Models.UserProfile;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppSample.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    readonly AuthenticationHeaderValue _authenticationHeaderValue;

    readonly IProxyHttpClientFactory _proxyHttpClientFactory;
    readonly ILogger<UserProfileRepository> _logger;
    readonly HttpRepositorySettings _upSettings;
    readonly IGlobalUrlHelper _globalUrlHelper;

    static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public UserProfileRepository(
        IOptions<HttpRepositoriesSettings> httpRepositoriesSettings,
        IProxyHttpClientFactory proxyHttpClientFactory,
        ILogger<UserProfileRepository> logger, IGlobalUrlHelper globalUrlHelper)
    {
        _upSettings = httpRepositoriesSettings.Value.UserProfile;
        _proxyHttpClientFactory = proxyHttpClientFactory;
        _logger = logger;
        _globalUrlHelper = globalUrlHelper;

        _authenticationHeaderValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_upSettings.UserName}:{_upSettings.UserPass}")));
    }

    public async Task<UserProfile?> GetUserProfile(long msisdn)
    {
        var result =
            await GetAsync<UserProfileResponse>($"ups?msisdn={msisdn}");

        return result?.CreateUserProfileEntity();
    }
    
    public async Task<PushPinResultState> SendMcPushPin(long msisdn, string message)
    {
        var requestModel = new PushPinRequest
        {
            Msisdn = msisdn,
            Text = message,
            CallbackUrl = _globalUrlHelper.GetMcPushPinAnswerUrl()
        };
        var response = await PostAsync<PushPinRequest, PushPinResponse>("mc/push-pin", requestModel);
        return response?.InitVerifyState ?? PushPinResultState.NoValue;
    }

    public async Task<PushPinResultState> SendDstkPushPin(long msisdn, string message)
    {
        var requestModel = new PushPinRequest
        {
            Msisdn = msisdn,
            Text = message,
            CallbackUrl = _globalUrlHelper.GetDstkPushPinAnswerUrl()
        };
        var response = await PostAsync<PushPinRequest, PushPinResponse>("dstk/push-pin", requestModel);
        return response?.InitVerifyState ?? PushPinResultState.NoValue;
    }

    public async Task SendHistory(HistoryCommand command)
    {
        var requestModel = new HistoryRequest(command);
        await PostAsync("history", requestModel);
    }

    async Task<TResult?> GetAsync<TResult>(string path) where TResult : struct
    {
        var userProfileGet = $"{_upSettings.Host}/{path}";
        var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.NoProxy, userProfileGet, false);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, userProfileGet)
        {
            Headers = { Authorization = _authenticationHeaderValue }
        };

        var response = await httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode) return null;

        var resultStream = await response.Content.ReadAsByteArrayAsync();
        var result = JsonSerializer.Deserialize<TResult>(resultStream, JsonSerializerOptions);

        return result;
    }

    async Task PostAsync<T>(string path, T requestModel) where T : struct
    {
        var userProfileGet = $"{_upSettings.Host}/{path}";
        var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.NoProxy, userProfileGet, false);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, userProfileGet)
        {
            Headers = { Authorization = _authenticationHeaderValue },
            Content = JsonContent.Create(requestModel, options: JsonSerializerOptions)
        };

        await httpClient.SendAsync(requestMessage);
    }

    async Task<TResult?> PostAsync<T, TResult>(string path, T requestModel) where T : struct where TResult : struct
    {
        var userProfileGet = $"{_upSettings.Host}/{path}";
        var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.NoProxy, userProfileGet, false);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, userProfileGet)
        {
            Headers = { Authorization = _authenticationHeaderValue },
            Content = JsonContent.Create(requestModel, options: JsonSerializerOptions)
        };

        var response = await httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode) return null;

        var resultStream = await response.Content.ReadAsByteArrayAsync();
        var result = JsonSerializer.Deserialize<TResult>(resultStream, JsonSerializerOptions);

        return result;
    }

    struct UserProfileResponse
    {
        public AppletResponse[]? applets { get; init; }
        public bool is_blocked { get; init; }
        public bool is_pin_active { get; init; }
        public int? pin { get; init; }

        public UserProfile CreateUserProfileEntity() => new()
        {
            Applets = applets?.Where(a => a.is_enabled).Select(a => a.id).ToArray(),
            IsBlocked = is_blocked,
            IsPinActive = is_pin_active,
            Pin = pin
        };
    }

    /// <summary>
    /// Модель для формирования запроса ввода ПИН-кода
    /// </summary>
    readonly struct PushPinRequest
    {
        /// <summary>
        /// MSISDN, на который будет отправлен запрос
        /// </summary>
        public long Msisdn { get; init; }

        /// <summary>
        /// Текст PUSH-сообщения LoA3
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// URL для асинхронного ответа результата
        /// </summary>
        public string CallbackUrl { get; init; }
    }

    /// <summary>
    /// Модель ответа на запрос ввода ПИН-кода
    /// </summary>
    readonly struct PushPinResponse
    {
        /// <summary>
        /// Состояние инициации запроса ввода ПИН-кода
        /// </summary>
        public PushPinResultState InitVerifyState { get; init; }
    }

    struct AppletResponse
    {
        public AppletType id { get; init; }
        public string name { get; init; }
        public string version { get; init; }
        public bool is_enabled { get; init; }
    }

    struct HistoryRequest
    {
        [JsonPropertyName("msisdn")] public string? Msisdn { get; init; }

        [JsonPropertyName("auth_status")] public string? AuthStatus { get; init; }

        [JsonPropertyName("sp_name")] public string? SpName { get; init; }

        [JsonPropertyName("auth_time")] public long? AuthTime { get; init; }

        [JsonPropertyName("pin_activate")] public bool? PinActivate { get; init; }

        [JsonPropertyName("pin")] public string? Pin { get; init; }

        [JsonPropertyName("blocked")] public bool? Blocked { get; init; }

        public HistoryRequest(HistoryCommand command)
        {
            Msisdn = command.Msisdn.ToString();
            SpName = command.SpName;
            AuthStatus = command.AuthStatus.ToString("G").ToLowerInvariant();
            AuthTime = command.AuthTime.ToUniversalTime().ToUnixTimeSeconds();
            PinActivate = command.PinActivate;
            Pin = command.Pin;
            Blocked = command.Blocked;
        }
    }
}