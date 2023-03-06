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
using AppSample.Domain.DAL;
using AppSample.Infrastructure.DAL;
using AppSample.Infrastructure.Services;
using AppSample.CoreTools.RedisSignal;

namespace AppSample.Admin;

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

        services.AddSingleton<IDbRepository, DbRepository>();
        services.AddSingleton<ICachedConfigService, CachedConfigService>();
        services.AddSingleton<IRedisSignalService, RedisSignalService>();

        services.AddSingleton<IAdminUserService, AdminUserService>();
        services.AddSingleton<IServiceProviderService, ServiceProviderService>();
        services.AddSingleton<IEntityService, EntityService>();

        if (useHangfire)
        {
            services.AddHangfire(x => x.UsePostgreSqlStorage(commonSettings.ConnectionString));
            GlobalConfiguration.Configuration.UsePostgreSqlStorage(commonSettings.ConnectionString);

            foreach (var type in TypesHelper.GetAllDescendantsInAppSampleAssemblies<BaseJob>())
            {
                services.AddSingleton(type);
            }
        }
    }
}