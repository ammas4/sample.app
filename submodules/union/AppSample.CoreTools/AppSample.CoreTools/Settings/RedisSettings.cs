namespace AppSample.CoreTools.Settings;

public class RedisSettings : BaseSettings, IRedisSettings
{
    public string ConnectionString { get; set; }
    public string ChannelPrefix { get; set; }
}