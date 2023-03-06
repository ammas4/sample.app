using System.Net.Http.Json;
using System.Text.Json;
using AppSample.CoreTools.Infrastructure;
using AppSample.CoreTools.Infrastructure.Interfaces;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.DeviceAdapter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppSample.Infrastructure.Repositories;

public class DeviceAdapterRepository : IDeviceAdapterRepository
{
    readonly IProxyHttpClientFactory _proxyHttpClientFactory;
    readonly ILogger<DeviceAdapterRepository> _logger;
    readonly HttpRepositorySettings _daSettings;
    readonly IGlobalUrlHelper _globalUrlHelper;

    static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public DeviceAdapterRepository(
        IOptions<HttpRepositoriesSettings> httpRepositoriesSettings,
        IProxyHttpClientFactory proxyHttpClientFactory,
        ILogger<DeviceAdapterRepository> logger, IGlobalUrlHelper globalUrlHelper)
    {
        _daSettings = httpRepositoriesSettings.Value.DeviceAdapter;
        _proxyHttpClientFactory = proxyHttpClientFactory;
        _logger = logger;
        _globalUrlHelper = globalUrlHelper;
    }

    public async Task<DaCommandResultType> SendPushToMc(long msisdn, string text)
    {
        var displayTextModel = new DisplayTextRequest
        {
            Msisdn = msisdn,
            Text = text,
            CallbackUrl = _globalUrlHelper.GetMcPushAnswerUrl()
        };

        var (isOk, result) =
            await Send<DisplayTextRequest, DaResponse>(displayTextModel, "oldmc/displaytext");

        if (!isOk) return DaCommandResultType.NoResult;

        if (!string.IsNullOrEmpty(result.Error))
            _logger.LogError(new InvalidOperationException("Push sending error"), result.Error);

        return result.ResultType;
    }

    public async Task<DaCommandResultType> SendPushToDstk(long msisdn, string text)
    {
        var displayTextModel = new DisplayTextRequest
        {
            Msisdn = msisdn,
            Text = text,
            CallbackUrl = _globalUrlHelper.GetDstkMcPushAnswerUrl()
        };

        var (isOk, result) =
            await Send<DisplayTextRequest, DaResponse>(displayTextModel, "dstk/displaytext");

        if (!isOk) return DaCommandResultType.NoResult;

        if (!string.IsNullOrEmpty(result.Error))
            _logger.LogError(new InvalidOperationException("Push sending error"), result.Error);

        return result.ResultType;
    }

    async Task<(bool isOk, TResult result)> Send<T, TResult>(T requestModel, string path)
        where TResult : struct
    {
        var daSendUrl = $"{_daSettings.Host}/{path}";
        var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.NoProxy, daSendUrl, false);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, daSendUrl)
        {
            Headers = { { "X-API-KEY", _daSettings.ApiKey } },
            Content = JsonContent.Create(requestModel, options: JsonSerializerOptions)
        };

        var response = await httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode) return (false, default);

        var resultStream = await response.Content.ReadAsByteArrayAsync();
        var result = JsonSerializer.Deserialize<TResult>(resultStream, JsonSerializerOptions);

        return (true, result);
    }

    public async Task<bool> SendSmsAsync(long msisdn, string text)
    {
        var url = $"{_daSettings.Host}/sms/send";
        var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.DefaultProxy, url, false);

        var requestModel = new SendSmsRequest
        {
            Msisdn = msisdn,
            Text = text
        };

        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestModel, options: JsonSerializerOptions)
        };

        var response = await httpClient.SendAsync(httpRequestMessage);
        if (!response.IsSuccessStatusCode) return false;

        var result = JsonSerializer.Deserialize<DaResponse>(await response.Content.ReadAsStreamAsync(),
            JsonSerializerOptions);

        if (!string.IsNullOrEmpty(result.Error))
            _logger.LogError(new InvalidOperationException("Sms sending error"), result.Error);

        return result.ResultType == DaCommandResultType.Sent;
    }

    struct DaResponse
    {
        public DaCommandResultType ResultType { get; init; }
        public string Error { get; init; }
    }

    struct DisplayTextRequest
    {
        public long Msisdn { get; init; }
        public string Text { get; init; }
        public string CallbackUrl { get; init; }
    }

    struct SendSmsRequest
    {
        public long Msisdn { get; init; }
        public string Text { get; init; }
    }
}