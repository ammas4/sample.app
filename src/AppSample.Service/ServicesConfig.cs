using AppSample.CoreTools.DapperContrib;
using AppSample.CoreTools.Helpers;
using AppSample.CoreTools.Jobs;
using AppSample.CoreTools.Logging;
using AppSample.CoreTools.Redis;
using AppSample.CoreTools.Settings;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AppSample.CoreTools.Infrastructure.Interfaces;
using AppSample.CoreTools.Infrastructure.Implementations;
using AppSample.CoreTools.Infrastructure;
using AppSample.Domain.Services.Authenticators;
using AppSample.Infrastructure.Services;
using AppSample.CoreTools.RedisSignal;

namespace AppSample.Service;

/// <summary>
/// 
/// </summary>
public static class ServicesConfig
{
    public static void Configure(IServiceCollection services, IConfiguration configuration, bool useHangfire = true)
    {
        PostgresqlDapperHelper.Configure();

        var commonSettings = configuration.GetSection(new CommonSettings().SectionName).Get<CommonSettings>();

        services.ConfigureAllSettings(configuration);

        services.AddRequestResponseLogger(sp => sp.GetRequiredService<IOptions<LogRequestResponseSettings>>().Value);
        services.AddRedisService(sp => sp.GetRequiredService<IOptions<RedisSettings>>().Value);

        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        services.AddSingleton<ISmsOtpAuthenticator, SmsOtpAuthenticator>();
        services.AddSingleton<ICachedConfigService, CachedConfigService>();
        services.AddSingleton<IRedisSignalService, RedisSignalService>();

        services.AddSingleton<IConsentSmsService, ConsentSmsService>();
        services.AddSingleton<ISIAuthorizeService, SIAuthorizeService>();
        services.AddSingleton<IAdminUserService, AdminUserService>();
        services.AddSingleton<IServiceProviderService, ServiceProviderService>();
        services.AddSingleton<ISubjectService, SubjectService>();
        services.AddSingleton<IEntityService, EntityService>();

        services.AddSingleton<IProxyHttpClientFactory, ProxyHttpClientFactory>();
        services.AddSingleton<SynchronizedMemoryCache>();

        services.AddHttpClient(NamedHttpClient.NoProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( false));
        services.AddHttpClient(NamedHttpClient.DefaultProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( false));
        services.AddHttpClient(NamedHttpClient.IdgwProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( false));
        services.AddHttpClient(NamedHttpClient.DiscoveryProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( false));
        services.AddHttpClient(NamedHttpClient.AllowedUntrustedSsl)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( true));
        services.AddHttpClient(NamedHttpClient.AllowedUntrustedSslNoProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( true));

        if (useHangfire)
        {
            services.AddHangfire(x => x.UsePostgreSqlStorage(commonSettings.ConnectionString));
            GlobalConfiguration.Configuration.UsePostgreSqlStorage(commonSettings.ConnectionString);

            foreach (var type in TypesHelper.GetAllDescendantsInBeelineAssemblies<BaseJob>())
            {
                services.AddSingleton(type);
            }
        }
    }


    /// <summary>
    /// Возвращает HttpClientHandler с настройками прокси и проверки Ssl
    /// </summary>
    static Func<IServiceProvider, HttpClientHandler>
        GetHttpHandlerSetter(bool allowUntrustedSsl) =>
        sp =>
        {
            HttpClientHandler handler = new() {AllowAutoRedirect = false};


            handler.Proxy = null;
            handler.UseProxy = false;

            if (allowUntrustedSsl)
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        };
}