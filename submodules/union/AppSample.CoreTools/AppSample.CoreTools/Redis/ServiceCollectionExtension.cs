using AppSample.CoreTools.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace AppSample.CoreTools.Redis;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRedisService(this IServiceCollection services, Func<IServiceProvider, IRedisSettings> settingsGenerator)
    {
        services.AddSingleton<IRedisSettings>(sp => settingsGenerator(sp));
        services.AddRedisService();
        return services;
    }

    public static IServiceCollection AddRedisService(this IServiceCollection services, IRedisSettings settings)
    {
        services.AddSingleton<IRedisSettings>(settings);
        services.AddRedisService();
        return services;
    }

    static IServiceCollection AddRedisService(this IServiceCollection services)
    {
        services.AddSingleton<IRedisService, RedisService>();
        return services;
    }
}