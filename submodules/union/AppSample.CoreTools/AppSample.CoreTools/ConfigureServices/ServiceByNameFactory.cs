using Microsoft.Extensions.DependencyInjection;

namespace AppSample.CoreTools.ConfigureServices;

internal class ServiceByNameFactory<TService> : IServiceByNameFactory<TService>
{
    readonly IServiceProvider _serviceProvider;
    readonly IDictionary<string, Type> _registrations;

    public ServiceByNameFactory(IServiceProvider serviceProvider, IDictionary<string, Type> registrations)
    {
        _serviceProvider = serviceProvider;
        _registrations = registrations;
    }

    public TService? GetByName(string name)
    {
        if (!_registrations.TryGetValue(name, out var implementationType))
            throw new ArgumentException($"Service name '{name}' is not registered");
        return (TService?) _serviceProvider.GetService(implementationType);
    }

    public TService GetRequiredByName(string name)
    {
        if (!_registrations.TryGetValue(name, out var implementationType))
            throw new ArgumentException($"Service name '{name}' is not registered");
        return (TService) _serviceProvider.GetRequiredService(implementationType);
    }

    public IDictionary<string, TService?> GetAll()
    {
        Dictionary<string, TService?> result = new Dictionary<string, TService?>();
        foreach (var pair in _registrations)
        {
            result.Add(pair.Key, (TService?) _serviceProvider.GetService(pair.Value));
        }

        return result;
    }
}