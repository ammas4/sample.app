namespace AppSample.CoreTools.Redis;

public interface IRedisSignalService
{
    /// <summary>
    /// обработчик изменения настроек системы
    /// </summary>
    event Action? OnStateChange;

    void SignalStateChange();

}