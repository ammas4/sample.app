namespace AppSample.CoreTools.Redis;

public class RedisServiceLockException : Exception
{
    public RedisServiceLockException(string message) : base(message)
    {
    }
}