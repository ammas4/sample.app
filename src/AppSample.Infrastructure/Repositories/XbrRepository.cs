using System.Net.Http.Json;
using AppSample.Domain.DAL;
using AppSample.Domain.Models;
using AppSample.CoreTools.Helpers;
using AppSample.CoreTools.Infrustructure;
using AppSample.CoreTools.Infrustructure.Interfaces;
using Microsoft.Extensions.Options;

namespace AppSample.Infrastructure.Repositories;

public class XbrRepository : IXbrRepository
{
    readonly IdgwSettings _idgwSettings;
    readonly IProxyHttpClientFactory _proxyHttpClientFactory;

    public XbrRepository(IOptions<IdgwSettings> idgwSettings,
        IProxyHttpClientFactory proxyHttpClientFactory)
    {
        _idgwSettings = idgwSettings.Value;
        _proxyHttpClientFactory = proxyHttpClientFactory;
    }

    public async Task<string?> GetMsisdnFromXbrTokenAsync(string xbrToken)
    {
        var authXbrUrlBuilder = new UrlBuilder(UrlHelper.Combine(_idgwSettings.UsssUrl, "/api/auth/xbr"));
        authXbrUrlBuilder.Query["xbrToken"] = xbrToken;
        var authXbrUrl = authXbrUrlBuilder.ToString();

        var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.NoProxy, authXbrUrl, false);
        XbrAuthenticationResponse? result = await httpClient.GetFromJsonAsync<XbrAuthenticationResponse>(authXbrUrl);

        if (result?.Meta != null && result.Value.Meta.Value.IsOkStatus() && string.IsNullOrEmpty(result.Value.Ctn) == false)
        {
            return result.Value.Ctn;
        }

        return null;
    }

    struct XbrAuthenticationResponse
    {
        public UsssMetaResponse? Meta { get; set; }

        public string? Token { get; set; }
        public string? Ctn { get; set; }
        public bool? XbrOfferAccepted { get; set; }
        public string? AcceptType { get; set; }
    }

    struct UsssMetaResponse
    {
        public string Status { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }

        public bool IsOkStatus() => string.Equals(Status, "OK", StringComparison.InvariantCultureIgnoreCase);
    }
}
