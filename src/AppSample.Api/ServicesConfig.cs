using System.Net;
using AppSample.CoreTools.DapperContrib;
using AppSample.CoreTools.Helpers;
using AppSample.CoreTools.Jobs;
using AppSample.CoreTools.Logging;
using AppSample.CoreTools.Redis;
using AppSample.CoreTools.Settings;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Options;
using AppSample.Infrastructure.DAL;
using AppSample.Infrastructure.Repositories;
using AppSample.CoreTools.ConfigureServices.OpenTelemetry.Settings;
using AppSample.CoreTools.ConfigureServices;
using AppSample.Infrastructure.Telemetry;
using AppSample.CoreTools.Infrastructure.Implementations;
using AppSample.CoreTools.Infrastructure.Interfaces;
using AppSample.CoreTools.Infrastructure;
using AppSample.Domain.Services.AuthenticationChain;
using AppSample.Domain.Services.AuthenticationResultHandlers;
using AppSample.Domain.Services.Authenticators;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using AppSample.Infrastructure.Services;
using AppSample.CoreTools.RedisSignal;

namespace AppSample.Api;

/// <summary>
/// 
/// </summary>
public static class ServicesConfig
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="useHangfire"></param>
    public static void Configure(IServiceCollection services, IConfiguration configuration, bool useHangfire = true)
    {
        PostgresqlDapperHelper.Configure();

        var commonSettings = configuration.GetSection(new CommonSettings().SectionName).Get<CommonSettings>();

        services.ConfigureAllSettings(configuration);

        services.AddRequestResponseLogger(sp => sp.GetRequiredService<IOptions<LogRequestResponseSettings>>().Value);
        services.AddRedisService(sp => sp.GetRequiredService<IOptions<RedisSettings>>().Value);

        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddSingleton<IGlobalUrlHelper, GlobalUrlHelper>();

        services.AddSingleton<ISmppDAL, SmppOneClientDAL>();
        services.AddSingleton<ISmsSmppRepository, SmsSmppRepository>();
        services.AddSingleton<IDeviceAdapterRepository, DeviceAdapterRepository>();
        services.AddSingleton<IUserProfileRepository, UserProfileRepository>();
        services.AddSingleton<ICachedConfigService, CachedConfigService>();
        services.AddSingleton<IRedisSignalService, RedisSignalService>();

        services.AddSingleton<IConsentSmsService, ConsentSmsService>();
        services.AddSingleton<ISIAuthorizeService, SIAuthorizeService>();
        services.AddSingleton<IDiAuthorizeService, DiAuthorizeService>();
        services.AddSingleton<IDiAuthStateService, DiAuthStateService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IAdminUserService, AdminUserService>();
        services.AddSingleton<IServiceProviderService, ServiceProviderService>();
        services.AddSingleton<ISubjectService, SubjectService>();
        services.AddSingleton<ISiAuthenticationResultHandler, SiAuthenticationResultHandler>();
        services.AddSingleton<IDiAuthenticationResultHandler, DiAuthenticationResultHandler>();
        services.AddSingleton<IAuthenticationChainSession, AuthenticationChainSession>();
        services.AddSingleton<IAuthenticationChainService, AuthenticationChainServiceService>();
        services.AddSingleton<IAuthChainJobScheduler, AuthChainJobScheduler>();
        services.AddSingleton<ISmsOtpAuthenticator, SmsOtpAuthenticator>();
        services.AddSingleton<ISmsUrlAuthenticator, SmsUrlAuthenticator>();
        services.AddSingleton<IUssdAuthenticator, UssdAuthenticator>();
        services.AddSingleton<ISeamlessAuthenticator, SeamlessAuthenticator>();
        services.AddSingleton<IMcPushAuthenticator, McPushAuthenticator>();
        services.AddSingleton<IDstkPushAuthenticator, DstkPushAuthenticator>();
        services.AddSingleton<IIPRangeService, IPRangeService>();

        services.AddSingleton<IDbRepository, DbRepository>();
        services.AddSingleton<IProxyHttpClientFactory, ProxyHttpClientFactory>();

        services.AddSingleton<ISmsHttpRepository, SmsHttpRepository>();
        services.AddSingleton<ISmppRepository, SmppRepository>();
        services.AddSingleton<IResourceServerService, ResourceServerService>();
        services.AddSingleton<IUssdRepository, UssdRepository>();
        services.AddSingleton<IDeviceAdapterRepository, DeviceAdapterRepository>();
        services.AddSingleton<IXbrRepository, XbrRepository>();

        services.AddSingleton<SynchronizedMemoryCache>();
        services.AddSingleton<IShortUrlService, ShortUrlService>();

        services.AddSingleton<IUpsService, UpsService>();
        services.AddSingleton<IEntityService, EntityService>();

        services.AddHttpClient(NamedHttpClient.NoProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( false,false));
        services.AddHttpClient(NamedHttpClient.DefaultProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( true,false));
        services.AddHttpClient(NamedHttpClient.IdgwProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( true,false));
        services.AddHttpClient(NamedHttpClient.DiscoveryProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( true,false));
        services.AddHttpClient(NamedHttpClient.AllowedUntrustedSsl)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter( true,true));
        services.AddHttpClient(NamedHttpClient.AllowedUntrustedSslNoProxy)
            .ConfigurePrimaryHttpMessageHandler(GetHttpHandlerSetter(false, true));

        if (useHangfire)
        {
            services.AddHangfire(x => x.UsePostgreSqlStorage(commonSettings.ConnectionString));
            GlobalConfiguration.Configuration.UsePostgreSqlStorage(commonSettings.ConnectionString);

            foreach (var type in TypesHelper.GetAllDescendantsInAppSampleAssemblies<BaseJob>())
            {
                services.AddSingleton(type);
            }
        }

        var telemetrySettings = configuration.GetSection(new TelemetrySettings().SectionName).Get<TelemetrySettings>();
        services.AddOpenTelemetry(telemetrySettings);
        services.AddSingleton<IBaseMetricsService, SiMetricsService>();
    }


    /// <summary>
    /// Возвращает HttpClientHandler с настройками прокси и проверки Ssl
    /// </summary>
    static Func<IServiceProvider, HttpClientHandler>
        GetHttpHandlerSetter(bool useProxy, bool allowUntrustedSsl, bool useLogging = true) =>
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