using AppSample.CoreTools.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppSample.CoreTools.Settings;

public static class ServiceCollectionExtension
{
    public static IServiceCollection ConfigureAllSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var types = TypesHelper.GetAllDescendantsInBeelineAssemblies<BaseSettings>();
        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is BaseSettings configurationInstance)
            {
                var configurationSection = configuration.GetSection(configurationInstance.SectionName);
                //вызов services.Configure<>()
                var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions).GetMethods()
                    .Where(x => x.Name == "Configure")
                    .Single(m => m.GetParameters().Length == 2)
                    .MakeGenericMethod(type);
                configureMethod.Invoke(null, new object[] {services, configurationSection});
            }
        }

        return services;
    }

}