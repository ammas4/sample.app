using StackExchange.Redis;

namespace AppSample.CoreTools.Redis;

public interface IRedisService
{
    int DeleteByPattern(string pattern);

    T? GetOrAddNotNullMessagePackedObject<T>(string key, Func<T?> valueFunc, TimeSpan expiry) where T : class, IMessagePackedObject;

    Task<T?> GetOrAddNotNullMessagePackedObjectAsync<T>(string key, Func<Task<T?>> valueFunc, TimeSpan expiry) where T : class, IMessagePackedObject;
    void Subscribe(string channelName, Action<string?> stateCallback);
    void Publish(string channelName, string update);

    bool SetString(string key, string value, TimeSpan expiry, CommandFlags flags = CommandFlags.None);
    Task<bool> SetStringAsync(string key, string value, TimeSpan expiry, CommandFlags flags = CommandFlags.None, When when = When.Always);

    string? GetString(string key);
    Task<string?> GetStringAsync(string key);

    string? GetAndSetString(string key, string newValue, TimeSpan expiry);
    Task<string?> GetAndSetStringAsync(string key, string newValue, TimeSpan expiry);
    Task<string?> GetAndSetStringAsync(string key, string newValue);


    bool IsExists(string key);
    Task<bool> IsExistsAsync(string key);

    long IncrementLong(string key, long incrementValue, TimeSpan expiry);
    Task<long> IncrementLongAsync(string key, long incrementValue, TimeSpan expiry);

    bool Delete(string key, CommandFlags flags = CommandFlags.None);
    Task<bool> DeleteAsync(string key, CommandFlags flags = CommandFlags.None);

    Task<string?> GetAndDeleteStringAsync(string key);

    Task<bool> AddToSortedSetAsync(string key, string member, double score);
    Task<IReadOnlyCollection<string>> GetSortedSetRangeByScoreAsync(string key, double startScore = double.NegativeInfinity,
        double stopScore = double.PositiveInfinity);
    Task<bool> RemoveFromSortedSetAsync(string key, string member);
    Task<long> RemoveFromSortedSetAsync(string key, IReadOnlyCollection<string>? members);
    Task<double?> GetSortedSetScoreAsync(string key, string member);
    Task<long> RemoveSortedSetRangeByScoreAsync(string key, double startScore = double.NegativeInfinity,
        double stopScore = double.PositiveInfinity);
    
    Task<bool> KeyExpiryAsync(string key, TimeSpan expiry, CommandFlags flags = CommandFlags.None);
    Task<TimeSpan?> KeyTimeToLiveAsync(string key, CommandFlags flags = CommandFlags.None);

    T? GetMessagePackedObject<T>(string key) where T : class, IMessagePackedObject;
    Task<T?> GetMessagePackedObjectAsync<T>(string key) where T : class, IMessagePackedObject;
}