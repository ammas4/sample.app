using System.Diagnostics;
using System.Text.RegularExpressions;
using AppSample.CoreTools.Infrustructure.Interfaces;
using AppSample.CoreTools.Settings;
using Microsoft.Extensions.Options;

namespace AppSample.CoreTools.Infrustructure.Implementations
{
    /// <summary>
    /// Фабрика создания HttpClient и WebRequest с учетом прокси
    /// </summary>
    /// <inheritdoc/>
    public class ProxyHttpClientFactory : IProxyHttpClientFactory
    {
        readonly ProxySettings _settings;
        readonly IHttpClientFactory _factory;

        public ProxyHttpClientFactory(IOptions<ProxySettings> settings, IHttpClientFactory factory)
        {
            _settings = settings.Value;
            _factory = factory;
        }

        public HttpClient CreateHttpClient(string name, string? url, bool allowUntrustedSsl)
        {
            var useProxy = true;
            
            if (string.IsNullOrEmpty(url) == false)
            {
                var uri = new Uri(url);
                var host = uri.Host;

                if (_settings.ExcludeHosts?.Any(s => Regex.IsMatch(host, WildCardToRegular(s), RegexOptions.IgnoreCase)) ?? false)
                {
                    useProxy = false;
                }
            }
            
            Activity.Current?.AddTag("mid.proxy", $"{useProxy}: {url}");

            return useProxy
                ? _factory.CreateClient(allowUntrustedSsl 
                    ? NamedHttpClient.AllowedUntrustedSsl 
                    : name)
                : _factory.CreateClient(allowUntrustedSsl
                    ? NamedHttpClient.AllowedUntrustedSslNoProxy
                    : NamedHttpClient.NoProxy);
        }

        static string WildCardToRegular(string hostPattern)
        {
            return "^" + Regex.Escape(hostPattern).Replace("\\*", ".*") + "$";
        }
    }
}
