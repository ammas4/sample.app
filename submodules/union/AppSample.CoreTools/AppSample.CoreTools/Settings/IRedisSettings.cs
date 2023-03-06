namespace AppSample.CoreTools.Settings;

public interface IRedisSettings 
{
    string ConnectionString { get;  }
    string ChannelPrefix { get;  }
}