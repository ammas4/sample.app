using AppSample.Domain.CachedConfig;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.CoreTools.Redis;
using AppSample.Domain.Models.Settings;
using Microsoft.Extensions.Logging;
using AppSample.CoreTools.RedisSignal;

namespace AppSample.Domain.Services;

public class CachedConfigService : ICachedConfigService
{
    readonly IDbRepository _dbRepository;
    volatile ConfigState _state;
    readonly IRedisSignalService _redisSignalService;
    readonly ILogger<CachedConfigService> _logger;

    readonly Timer _reloadTimer;
    readonly CancellationTokenSource _cts = new();

    public CachedConfigService(IDbRepository dbRepository, IRedisSignalService redisSignalSignalService, ILogger<CachedConfigService> logger)
    {
        _dbRepository = dbRepository;
        _redisSignalService = redisSignalSignalService;
        _logger = logger;

        _redisSignalService.OnStateChange += (str) =>
        {
            _logger.LogDebug("Configuration change signal");
            Load();
        };

        _reloadTimer = new Timer(TimerCallback, null, Timeout.Infinite, 0);
        Load();
    }

    public void Stop()
    {
        _cts.Cancel();
    }


    public ConfigState GetState()
    {
        return _state;
    }

    public void SignalChange()
    {
        _redisSignalService.SignalStateChange();
    }

    ConfigState GetStateFromDatabase()
    {
        var settings =_dbRepository.GetSettings().ToDictionary(s => s.Name, s => s.Value);
        
        var isMigrationSmsUrlForced = bool.TryParse(settings[SettingsNames.MigrationForceSmsUrl], out var migrationForceSmsUrl) && migrationForceSmsUrl;
        
        var state = new ConfigState
        {
            ServiceProviders = _dbRepository.GetAllServiceProviders(isMigrationSmsUrlForced)
                .Where(x => x is { Active: true, Deleted: false })
                .ToList(),
            Settings = settings,
            IsMigrationSmsUrlForced = isMigrationSmsUrlForced
        };

        foreach (var item in state.ServiceProviders)
        {
            if (string.IsNullOrEmpty(item.ClientId) == false)
                state.ServiceProvidersByClientId[item.ClientId] = item;
            state.ServiceProvidersById[item.Id] = item;
        }

        

        return state;
    }

    void Load()
    {
        _logger.LogDebug("Loading configuration");

        _state = GetStateFromDatabase();
        _reloadTimer.Change(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
    }

    /// <summary>
    /// Callback от таймера
    /// </summary>
    /// <param name="state"></param>
    void TimerCallback(object? state)
    {
        _logger.LogInformation($"Timer callback");

        if (_cts.IsCancellationRequested) return;
        _reloadTimer.Change(Timeout.Infinite, 0);
        try
        {
            Load();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background load error");
            _reloadTimer.Change(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
        }
    }
}