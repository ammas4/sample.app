using Microsoft.Extensions.DependencyInjection;

namespace AppSample.CoreTools.Logging;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRequestResponseLogger(this IServiceCollection services, Func<IServiceProvider, ILogRequestResponseSettings> settingsGenerator)
    {
        services.AddSingleton<ILogRequestResponseSettings>(settingsGenerator);
        return services.AddRequestResponseLogger();
    }

    public static IServiceCollection AddRequestResponseLogger(this IServiceCollection services, ILogRequestResponseSettings settings)
    {
        services.AddSingleton<ILogRequestResponseSettings>(settings);
        return services.AddRequestResponseLogger();
    }

    static IServiceCollection AddRequestResponseLogger(this IServiceCollection services)
    {
        services.AddSingleton<IRequestResponseLogger, RequestResponseLogger>();
        services.AddSingleton<IHttpLogDataFinder, DefaultHttpLogDataFinder>();
        return services;
    }
}