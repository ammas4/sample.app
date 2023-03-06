namespace AppSample.CoreTools.ConfigureServices;

/// <summary>
/// Provides instances of registered services by name
/// </summary>
/// <typeparam name="TService"></typeparam>
public interface IServiceByNameFactory<TService>
{
    /// <summary>
    /// Provides instance of registered service by name or null if the named type isn't registered in <see cref="System.IServiceProvider"/>
    /// </summary>
    TService? GetByName(string name);

    /// <summary>
    /// Provides instance of registered service by name.  If type isn't registered an InvalidOperationException will be thrown.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    TService GetRequiredByName(string name);

    IDictionary<string, TService?> GetAll();
}