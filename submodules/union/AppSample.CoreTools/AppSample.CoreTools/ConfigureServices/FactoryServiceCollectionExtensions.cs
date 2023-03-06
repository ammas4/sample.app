using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSample.CoreTools.ConfigureServices;

public static class FactoryServiceCollectionExtensions
{
    /// <summary>
    /// Entry point for name-based registrations. This method should be called in order to start building
    /// named registrations for <typeparamref name="TService"/>"/>
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns><see cref="ServicesByNameBuilder&lt;TService&gt;"/> which is used to build multiple named registrations</returns>
    public static ServicesByNameBuilder<TService> AddByName<TService>(this IServiceCollection services)
    {
        return new ServicesByNameBuilder<TService>(services);
    }

    /// <summary>
    /// Provides instances of named registration or null if named type is not registered in <paramref name="provider"/>.
    /// </summary>
    /// <returns></returns>
    public static TService? GetServiceByName<TService>(this IServiceProvider provider, string name)
    {
        var factory = GetFactory<TService>(provider);
        return factory.GetByName(name);
    }

    /// <summary>
    /// Provides instances of named registration. Throws InvalidOperationException if named type is not registered in <paramref name="provider"/>.
    /// </summary>
    /// <returns></returns>
    public static TService GetRequiredServiceByName<TService>(this IServiceProvider provider, string name)
    {
        var factory = GetFactory<TService>(provider);
        return factory.GetRequiredByName(name);
    }

    public static IDictionary<string, TService?> GetAllServicesWithNames<TService>(this IServiceProvider provider)
    {
        var factory = GetFactory<TService>(provider);
        return factory.GetAll();
    }

    static IServiceByNameFactory<TService> GetFactory<TService>(IServiceProvider provider)
    {
        var factory = provider.GetService<IServiceByNameFactory<TService>>();
        if (factory == null)
            throw new InvalidOperationException(
                $"The factory {typeof(IServiceByNameFactory<TService>)} is not registered. Please use {nameof(FactoryServiceCollectionExtensions)}.{nameof(AddByName)}() to register names.");
        return factory;
    }
}