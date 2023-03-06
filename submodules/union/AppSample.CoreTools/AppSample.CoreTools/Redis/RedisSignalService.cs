using Microsoft.Extensions.Logging;

namespace AppSample.CoreTools.Redis;

public class RedisSignalService : IRedisSignalService
{
    private readonly IRedisService _redisService;
    private readonly ILogger<RedisSignalService> _logger;

    private const string ChannelName = "State";

    public RedisSignalService(IRedisService redisService,ILogger<RedisSignalService> logger)
    {
        _redisService = redisService;
        _logger = logger;

       _redisService.Subscribe(ChannelName, StateCallback);
    }

    private void StateCallback(string? message)
    {
        try
        {
            _logger.LogDebug("StateCallback: message=" + message);
            OnStateChange?.Invoke();
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "Error in StateCallback()");
        }
    }

    public event Action? OnStateChange;

    public void SignalStateChange()
    {
        _redisService.Publish(ChannelName, "update");
    }
}