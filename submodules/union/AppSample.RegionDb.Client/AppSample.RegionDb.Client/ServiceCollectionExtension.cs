using Beeline.MobileID.RegionDb.Client.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Beeline.MobileID.RegionDb.Client;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRegionDbClient(this IServiceCollection services, Func<IServiceProvider, IRegionDbSettings> settingsGenerator)
    {
        services.AddSingleton<IRegionDbSettings>(settingsGenerator);
        services.AddSingleton<IRegionDbClient, RegionDbClient>();
        return services;
    }
}