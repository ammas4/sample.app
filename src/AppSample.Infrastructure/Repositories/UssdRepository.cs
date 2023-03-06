using AppSample.CoreTools.Infrastructure;
using AppSample.CoreTools.Infrastructure.Interfaces;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace AppSample.Infrastructure.Repositories;

public class UssdRepository : IUssdRepository
{
    readonly IProxyHttpClientFactory _proxyHttpClientFactory;
    readonly UssdSettings _ussdSettings;
    readonly IGlobalUrlHelper _globalUrlHelper;

    public UssdRepository(
        IOptions<UssdSettings> ussdSettings,
        IProxyHttpClientFactory proxyHttpClientFactory,
        IGlobalUrlHelper globalUrlHelper)
    {
        _ussdSettings = ussdSettings.Value;
        _proxyHttpClientFactory = proxyHttpClientFactory;
        _globalUrlHelper = globalUrlHelper;
    }
    
    public async Task<(bool isOk, UssdFailure failureCode)> AskUserForConsistAsync(string msisdn)
    {
        var guid = Guid.NewGuid(); // зачем он нужен?
        var callbackUrl = _globalUrlHelper.GetUssdAskUserTextUrl();
        var ussdTextUrl = callbackUrl.Remove(callbackUrl.LastIndexOf(".ru", StringComparison.Ordinal));
        
        var ussdCenterUrl = $"{_ussdSettings.UssdCenterDomain}/index.html?msisdn={msisdn}&requestID={guid}&url={ussdTextUrl}";
        var httpClient = _proxyHttpClientFactory.CreateHttpClient(NamedHttpClient.NoProxy, ussdCenterUrl, false);
        
        var message = new HttpRequestMessage(HttpMethod.Get, ussdCenterUrl);
        
        var response = await httpClient.SendAsync(message);

        var (isOk, failureCode) = ParseUssdResponseHtml(await response.Content.ReadAsStreamAsync());

        return (isOk, (UssdFailure)failureCode);
    }

    (bool isOk, byte failureCode) ParseUssdResponseHtml(Stream htmlStream)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.Load(htmlStream);

        var h1 = htmlDoc.DocumentNode.SelectSingleNode("//body//h1");
        if (h1 != null)
        {
            if (h1.InnerText.ToLower() == "ok")
                return (true, default);
            
            throw new InvalidOperationException(GetParseUssdExceptionText(htmlStream));
        }

        var title = htmlDoc.DocumentNode.SelectSingleNode("//head//title");

        if (title == null) throw new InvalidOperationException(GetParseUssdExceptionText(htmlStream));
        
        const string resultCodePrefixStr = "Request failure: ";
        var titleSpan = title.InnerText.AsSpan();
        var resultCodePrefixIndex = titleSpan.IndexOf(resultCodePrefixStr, StringComparison.Ordinal);
        var resultCodeIndex = resultCodePrefixIndex + resultCodePrefixStr.Length;

        var resultCodeSpan = titleSpan[resultCodeIndex..titleSpan.Length];
        if (byte.TryParse(resultCodeSpan, out var code))
            return (false, code);

        throw new InvalidOperationException(GetParseUssdExceptionText(htmlStream));
    }

    static string GetParseUssdExceptionText(Stream htmlStream)
    {
        htmlStream.Position = 0;
        return $"Не удалось распарсить HTML от USSD-центра: {new StreamReader(htmlStream).ReadToEnd()}";
    }
}