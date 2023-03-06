using AppSample.Domain.CachedConfig;

namespace AppSample.Domain.Interfaces;

public interface ICachedConfigService
{
    void Stop();
    ConfigState GetState();
    void SignalChange();
}