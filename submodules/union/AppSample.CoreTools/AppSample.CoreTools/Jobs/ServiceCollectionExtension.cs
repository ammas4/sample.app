using AppSample.CoreTools.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace AppSample.CoreTools.Jobs;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddAllJobs(this IServiceCollection services)
    {
        foreach (Type type in TypesHelper.GetAllDescendantsInBeelineAssemblies<BaseJob>())
        {
            services.AddSingleton(type);
        }
        return services;
    }
}