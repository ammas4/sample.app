using Microsoft.Extensions.DependencyInjection;

namespace AppSample.CoreTools.ConfigureServices;

/// <summary>
/// Provides easy fluent methods for building named registrations of the same interface
/// </summary>
/// <typeparam name="TService"></typeparam>
public class ServicesByNameBuilder<TService>
{
    readonly IServiceCollection _services;

    readonly IDictionary<string, Type> _registrations;

    internal ServicesByNameBuilder(IServiceCollection services)
    {
        _services = services;
        _registrations = new Dictionary<string, Type>();
    }

    /// <summary>
    /// Maps name to corresponding implementation.
    /// Note that this implementation has to be also registered in IoC container so
    /// that <see cref="IServiceByNameFactory&lt;TService&gt;"/> is be able to resolve it.
    /// </summary>
    public ServicesByNameBuilder<TService> Add(string name, Type implementationType)
    {
        _registrations.Add(name, implementationType);
        return this;
    }

    /// <summary>
    /// Generic version of <see cref="Add"/>
    /// </summary>
    public ServicesByNameBuilder<TService> Add<TImplementation>(string name)
        where TImplementation : TService
    {
        return Add(name, typeof(TImplementation));
    }

    /// <summary>
    /// Adds <see cref="IServiceByNameFactory&lt;TService&gt;"/> to IoC container together with all registered implementations
    /// so it can be consumed by client code later.
    /// </summary>
    public void BuildAsTransient()
    {
        var registrations = _registrations; //registrations are shared across all instances
        _services.AddTransient<IServiceByNameFactory<TService>>(s => new ServiceByNameFactory<TService>(s, registrations));
        foreach (var registration in registrations)
        {
            _services.AddTransient(registration.Value);
        }
    }

    /// <summary>
    /// Adds <see cref="IServiceByNameFactory&lt;TService&gt;"/> to IoC container together with all registered implementations
    /// so it can be consumed by client code later.
    /// </summary>
    public void BuildAsScoped()
    {
        var registrations = _registrations; //registrations are shared across all instances
        _services.AddScoped<IServiceByNameFactory<TService>>(s => new ServiceByNameFactory<TService>(s, registrations));
        foreach (var registration in registrations)
        {
            _services.AddScoped(registration.Value);
        }
    }

    /// <summary>
    /// Adds <see cref="IServiceByNameFactory&lt;TService&gt;"/> to IoC container together with all registered implementations
    /// so it can be consumed by client code later.
    /// </summary>
    public void BuildAsSingleton()
    {
        var registrations = _registrations;
        _services.AddSingleton<IServiceByNameFactory<TService>>(s => new ServiceByNameFactory<TService>(s, registrations));
        foreach (var registration in registrations)
        {
            _services.AddSingleton(registration.Value);
        }
    }
}