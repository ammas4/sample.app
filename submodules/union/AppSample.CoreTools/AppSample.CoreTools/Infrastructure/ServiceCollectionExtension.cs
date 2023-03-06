using System.Net;
using AppSample.CoreTools.Infrustructure;
using AppSample.CoreTools.Logging;
using AppSample.CoreTools.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AppSample.CoreTools.Infrastructure;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Добавление HttpClient для каждого имени из NamedHttpClient
    /// </summary>
    /// <param name="services"></param>
    /// <param name="useLogging"></param>
    /// <returns></returns>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, bool useLogging = true)
    {
        services.AddHttpClient(NamedHttpClient.NoProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter(false, false, useLogging));
        services.AddHttpClient(NamedHttpClient.DefaultProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter(true, false, useLogging));
        services.AddHttpClient(NamedHttpClient.IdgwProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter(true, false, useLogging));
        services.AddHttpClient(NamedHttpClient.DiscoveryProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter(true, false, useLogging));
        services.AddHttpClient(NamedHttpClient.AllowedUntrustedSsl)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter(true, true, useLogging));
        services.AddHttpClient(NamedHttpClient.AllowedUntrustedSslNoProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter(false, true, useLogging));

        return services;
    }

    /// <summary>
    /// Возвращает HttpClientHandler с настройками прокси и проверки Ssl
    /// </summary>
    static Func<IServiceProvider, HttpClientHandler> GetHttpHandlerSetter(bool useProxy, bool allowUntrustedSsl, bool useLogging = true) =>
        sp =>
        {
            HttpClientHandler handler;
            if (useLogging)
            {
                var requestResponseLogger = sp.GetRequiredService<IRequestResponseLogger>();
                handler = new ApiLoggingHandler(requestResponseLogger);
            }
            else
            {
                handler = new();
            }

            handler.AllowAutoRedirect = false;

            var proxySettings = sp.GetRequiredService<IOptions<ProxySettings>>().Value;

            if (useProxy && string.IsNullOrEmpty(proxySettings.Address) == false)
            {
                handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
                handler.UseProxy = true;
                handler.Proxy = new WebProxy(proxySettings.Address);

                if (string.IsNullOrEmpty(proxySettings.UserName) == false)
                {
                    handler.Proxy.Credentials = new NetworkCredential(proxySettings.UserName, proxySettings.Password, proxySettings.Domain);
                }
            }
            else
            {
                handler.UseProxy = false;
                handler.Proxy = null;
            }

            if (allowUntrustedSsl)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        };
}
