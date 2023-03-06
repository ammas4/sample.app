using System.Buffers;
using AppSample.CoreTools.Helpers;
using AppSample.CoreTools.Settings;
using MessagePack;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AppSample.CoreTools.Redis;

public class RedisService : IRedisService
{
    readonly ILogger<RedisService> _logger;
    readonly IRedisSettings _redisSettings;
    readonly ConnectionMultiplexer _connection;

    public RedisService(IRedisSettings redisSettings, ILogger<RedisService> logger)
    {
        _redisSettings = redisSettings;
        _logger = logger;

        string connectionString = _redisSettings.ConnectionString;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Empty 'RedisConnection'");
        }

        var config = ConfigurationOptions.Parse(connectionString);
        config.AbortOnConnectFail = false;

        _connection = ConnectionMultiplexer.Connect(config);
        _logger.LogDebug($"Connect to Redis, config={connectionString}");
    }

    string AddChannelPrefix(string key)
    {
        return $"{_redisSettings.ChannelPrefix}_{key}";
    }

    public void Subscribe(string channelName, Action<string?> stateCallback)
    {
        ISubscriber sub = _connection.GetSubscriber();
        var fullChannelName = AddChannelPrefix(channelName);
        sub.Subscribe(fullChannelName).OnMessage(x => ChannelMessageCallback(x, stateCallback));
        _logger.LogDebug($"Subscribe to channel {fullChannelName}");
    }

    public void Publish(string channelName, string message)
    {
        ISubscriber sub = _connection.GetSubscriber();
        var fullChannelName = AddChannelPrefix(channelName);
        sub.Publish(fullChannelName, message, CommandFlags.FireAndForget);
    }

    void ChannelMessageCallback(ChannelMessage channelMessage, Action<string> messageAction)
    {
        try
        {
            string message = channelMessage.Message.ToString();
            messageAction(message);
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "ChannelMessageCallback error");
        }
    }

    /// <summary>
    /// Удаление ключей по паттерну (например "key*")
    /// Медленная операция, только для использования при отладке, тестировании или развертывании.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public int DeleteByPattern(string pattern)
    {
        var database = _connection.GetDatabase();
        int count = 0;
        foreach (var ep in _connection.GetEndPoints())
        {
            var server = _connection.GetServer(ep);
            var keys = server.Keys(pattern: pattern).ToArray();
            count += (int)database.KeyDelete(keys);
        }

        return count;
    }

    public bool CheckConnectionActive()
    {
        try
        {
            var res = _connection.IsConnected;
            return res;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Выполнение действия под глобальной блокировкой
    /// </summary>
    /// <param name="key">ключ блокировки</param>
    /// <param name="lockExpiry">время экспирации блокировки</param>
    /// <param name="limit">лимит времени на получение блокировки</param>
    /// <param name="action">выполняемое действие</param>
    /// <returns></returns>
    public async Task DoActionUnderLockAsync(string key, TimeSpan lockExpiry, TimeSpan limit, Func<Task> action)
    {
        await DoActionUnderLockAsync(key, lockExpiry, limit, (extendLockAction) => action());
    }

    /// <summary>
    /// Выполнение действия под глобальной блокировкой
    /// </summary>
    /// <param name="key">ключ блокировки</param>
    /// <param name="lockExpiry">время экспирации блокировки</param>
    /// <param name="limit">лимит времени на получение блокировки</param>
    /// <param name="action">выполняемое действие. В параметре передается метод продления блокировки</param>
    /// <returns></returns>
    public async Task DoActionUnderLockAsync(string key, TimeSpan lockExpiry, TimeSpan limit, Func<Action, Task> action)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentOutOfRangeException(nameof(key));
        if (lockExpiry < TimeSpan.FromSeconds(1)) throw new ArgumentOutOfRangeException(nameof(lockExpiry));

        var db = _connection.GetDatabase();

        string token = Guid.NewGuid().ToString();
        var startTime = TickCountHelper.GetUpTime();
        bool wasLockAttempt = false;
        do
        {
            if (wasLockAttempt)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
            }

            if (db.LockTake(key, token, lockExpiry))
            {
                void extendLockAction()
                {
                    db.LockExtend(key, token, lockExpiry);
                } // метод продления блокировки

                try
                {
                    await action(extendLockAction);
                }
                finally
                {
                    db.LockRelease(key, token);
                }

                return;
            }

            wasLockAttempt = true;
        } while (TickCountHelper.GetUpTime() - startTime < limit);

        throw new RedisServiceLockException($"Unable to acquire lock after {limit.TotalSeconds} seconds");
    }

    /// <summary>
    /// Получение объекта, ранее сохраненного с использованием сontractless сериализации MessagePack.
    /// Возвращается default, если такой записи нет или в случае ошибки десериализации
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Get<T>(string key)
    {
        using var lease = _connection.GetDatabase().StringGetLease(key);
        return DeserializeMessagePackContractless<T>(lease, key);
    }

    /// <summary>
    /// Получение объекта, ранее сохраненного с использованием сontractless сериализации MessagePack.
    /// Возвращается default, если такой записи нет или в случае ошибки десериализации
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> GetAsync<T>(string key)
    {
        using var lease = await _connection.GetDatabase().StringGetLeaseAsync(key);
        return DeserializeMessagePackContractless<T>(lease, key);
    }

    /// <summary>
    /// Получение строки
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string? GetString(string key)
    {
        var result = _connection.GetDatabase().StringGet(key);
        var text = (string)result;
        return text;
    }

    public long? GetLong(string key)
    {
        var result = _connection.GetDatabase().StringGet(key);
        if (result == RedisValue.Null) return null;
        long value = (long)result;
        return value;
    }

    /// <summary>
    /// Получение строки
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<string?> GetStringAsync(string key)
    {
        var result = await _connection.GetDatabase().StringGetAsync(key);
        var text = (string)result;
        return text;
    }

    /// <summary>
    /// Добавление объекта в set с использованием contractless сериализации MessagePack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> SetAddAsync<T>(string key, T value)
    {
        var redisValue = SerializeMessagePackContractless(value);
        var result = await _connection.GetDatabase().SetAddAsync(key, redisValue);
        return result;
    }

    /// <summary>
    /// Получение количества элементов в list
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<long> ListLengthAsync(string key)
    {
        var result = await _connection.GetDatabase().ListLengthAsync(key, CommandFlags.None);
        return result;
    }

    /// <summary>
    /// Получение массива всех объектов из set с использованием contractless сериализации MessagePack.
    /// В случае ошибки десериализации в элементе массиве будет default.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<List<T>> GetAllSetMembersAsync<T>(string key)
    {
        var result = await _connection.GetDatabase().SetMembersAsync(key, CommandFlags.None);
        return result.Select(item => DeserializeMessagePackContractless<T>(item, key)).ToList();
    }

    /// <summary>
    /// Установка срока действия ключа
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> KeyExpiryAsync(string key, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
    {
        var result = await _connection.GetDatabase().KeyExpireAsync(key, expiry, flags);
        return result;
    }
    
    public async Task<TimeSpan?> KeyTimeToLiveAsync(string key, CommandFlags flags = CommandFlags.None)
    {
        var database = _connection.GetDatabase();
        var ttl = await database.KeyTimeToLiveAsync(key, flags);
        return ttl;
    }

    /// <summary>
    /// Установка срока действия ключа
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiry"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public bool KeyExpiry(string key, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
    {
        var result = _connection.GetDatabase().KeyExpire(key, expiry, flags);
        return result;
    }

    /// <summary>
    /// Запись строки
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiry"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public bool SetString(string key, string value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
    {
        var result = _connection.GetDatabase().StringSet(key, value, expiry, flags: flags);
        return result;
    }

    /// <summary>
    /// Запись строки
    /// </summary>
    public async Task<bool> SetStringAsync(string key, string value, TimeSpan expiry, CommandFlags flags = CommandFlags.None, When when = When.Always)
    {
        return await _connection.GetDatabase().StringSetAsync(key, value, expiry, flags: flags, when: when);
    }

    /// <summary>
    /// Сохранение объекта с использованием сontractless сериализации MessagePack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiry"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public bool Set<T>(string key, T value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
    {
        var serialized = SerializeMessagePackContractless(value);
        return _connection.GetDatabase().StringSet(key, serialized, expiry, flags: flags);
    }

    /// <summary>
    /// Сохранение объекта с использованием сontractless сериализации MessagePack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan expiry)
    {
        var serialized = SerializeMessagePackContractless(value);
        return await _connection.GetDatabase().StringSetAsync(key, serialized, expiry);
    }

    /// <summary>
    /// Удаление по ключу
    /// </summary>
    /// <param name="key"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public bool Delete(string key, CommandFlags flags = CommandFlags.None)
    {
        var result = _connection.GetDatabase().KeyDelete(key, flags);
        return result;
    }

    public async Task<bool> DeleteAsync(string key, CommandFlags flags = CommandFlags.None)
    {
        var result = await _connection.GetDatabase().KeyDeleteAsync(key, flags);
        return result;
    }

    /// <summary>
    /// Удаление по ключам
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public int Delete(List<string> keys, CommandFlags flags = CommandFlags.None)
    {
        int count = keys.Count;
        var redisKeys = new RedisKey[count];
        for (int i = 0; i < count; i++)
        {
            redisKeys[i] = keys[i];
        }

        var result = _connection.GetDatabase().KeyDelete(redisKeys, flags);
        return (int)result;
    }

    /// <summary>
    /// Получение объекта с использованием десериализации MessagePack
    /// Возвращается default, если такой записи нет или в случае ошибки десериализации
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? GetMessagePackedObject<T>(string key) where T : class, IMessagePackedObject
    {
        using var lease = _connection.GetDatabase().StringGetLease(key);
        return DeserializeMessagePackObject<T>(lease, key);
    }

    /// <summary>
    /// Получение объекта с использованием десериализации MessagePack
    /// Возвращается default, если такой записи нет или в случае ошибки десериализации
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T?> GetMessagePackedObjectAsync<T>(string key) where T : class, IMessagePackedObject
    {
        using var lease = await _connection.GetDatabase().StringGetLeaseAsync(key);
        return DeserializeMessagePackObject<T>(lease, key);
    }

    /// <summary>
    /// Получение объекта, ранее сохраненного с использованием сontractless сериализации MessagePack.
    /// Если объекта нет - то он создается и сохраняется.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="valueFunc"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public T GetOrAdd<T>(string key, Func<T> valueFunc, TimeSpan expiry)
    {
        var data = Get<T>(key);
        if (EqualityComparer<T>.Default.Equals(data, default) == false)
            return data;

        var item = valueFunc(); //создание объекта
        Set(key, item, expiry);
        return item;
    }

    /// <summary>
    /// Получение объекта, ранее сохраненного с использованием сериализации MessagePack.
    /// Если объекта нет - то он создается и сохраняется.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="valueFunc"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public T GetOrAddMessagePackedObject<T>(string key, Func<T> valueFunc, TimeSpan expiry) where T : class, IMessagePackedObject
    {
        var data = GetMessagePackedObject<T>(key);
        if (EqualityComparer<T>.Default.Equals(data, default) == false)
            return data;

        var item = valueFunc(); //создание объекта
        SetMessagePackedObject(key, item, expiry);
        return item;
    }

    /// <summary>
    /// Получение объекта, ранее сохраненного с использованием сериализации MessagePack.
    /// Если объекта нет - то он создается и сохраняется.
    /// Если при создании объекта возвращается null, запись в кэш не происходит
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="valueFunc"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public T? GetOrAddNotNullMessagePackedObject<T>(string key, Func<T?> valueFunc, TimeSpan expiry) where T : class, IMessagePackedObject
    {
        var data = GetMessagePackedObject<T>(key);
        if (EqualityComparer<T>.Default.Equals(data, default) == false)
            return data;

        T? item = valueFunc(); //создание объекта
        if (item != null) SetMessagePackedObject(key, item, expiry);
        return item;
    }

    /// <summary>
    /// Получение объекта, ранее сохраненного с использованием сериализации MessagePack.
    /// Если объекта нет - то он создается и сохраняется.
    /// Если при создании объекта возвращается null, запись в кэш не происходит
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="valueFunc"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public async Task<T?> GetOrAddNotNullMessagePackedObjectAsync<T>(string key, Func<Task<T?>> valueFunc, TimeSpan expiry) where T : class, IMessagePackedObject
    {
        var data = GetMessagePackedObject<T>(key);
        if (EqualityComparer<T>.Default.Equals(data, default) == false)
            return data;

        T? item = await valueFunc(); //создание объекта
        if (item != null) SetMessagePackedObject(key, item, expiry);
        return item;
    }

    /// <summary>
    /// Запись объекта с использованием сериализации MessagePack 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public bool SetMessagePackedObject<T>(string key, T value, TimeSpan expiry) where T : class, IMessagePackedObject
    {
        RedisValue serialized = SerializeMessagePackObject(value);
        return _connection.GetDatabase().StringSet(key, serialized, expiry);
    }
        
    /// <summary>
    /// Проверка существования ключа
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsExists(string key)
    {
        return _connection.GetDatabase().KeyExists(key);
    }

    public async Task<bool> IsExistsAsync(string key)
    {
        return await _connection.GetDatabase().KeyExistsAsync(key);
    }

    /// <summary>
    /// Увеличение числа на указанное значение, возврат увеличенного значения.
    /// Если ключа ещё не было - число считается равным 0.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="incrementValue"></param>
    /// <param name="expiry"></param>
    /// <returns>значение после увеличения</returns>
    public long IncrementLong(string key, long incrementValue, TimeSpan expiry)
    {
        var database = _connection.GetDatabase();
        var result = database.StringIncrement(key, incrementValue);
        database.KeyExpire(key, expiry, CommandFlags.FireAndForget);
        return result;
    }

    public async Task<long> IncrementLongAsync(string key, long incrementValue, TimeSpan expiry)
    {
        var database = _connection.GetDatabase();
        var result = await database.StringIncrementAsync(key, incrementValue);
        database.KeyExpire(key, expiry, CommandFlags.FireAndForget);
        return result;
    }

    /// <summary>
    /// Атомарное получение и запись строки.
    /// Возвращается старая строка, если ключа ещё не было - то null.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="newValue"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public string? GetAndSetString(string key, string newValue, TimeSpan expiry)
    {
        var database = _connection.GetDatabase();
        var result = database.StringGetSet(key, newValue);
        database.KeyExpire(key, expiry, CommandFlags.FireAndForget);
        return result;
    }

    public async Task<string?> GetAndSetStringAsync(string key, string newValue, TimeSpan expiry)
    {
        var database = _connection.GetDatabase();
        var result = await database.StringGetSetAsync(key, newValue);
        database.KeyExpire(key, expiry, CommandFlags.FireAndForget);
        return result;
    }

    public async Task<string?> GetAndSetStringAsync(string key, string newValue)
    {
        var database = _connection.GetDatabase();
        var result = await database.StringGetSetAsync(key, newValue);
        return result;
    }
    public async Task<string?> GetAndDeleteStringAsync(string key)
    {
        var database = _connection.GetDatabase();
        var result = await database.StringGetDeleteAsync(key);
        return result;
    }

    public async Task<bool> AddToSortedSetAsync(string key, string member, double score)
    {
        var database = _connection.GetDatabase();
        return await database.SortedSetAddAsync(key, member, score, CommandFlags.FireAndForget);
    }

    public async Task<bool> RemoveFromSortedSetAsync(string key, string member)
    {
        var database = _connection.GetDatabase();
        return await database.SortedSetRemoveAsync(key, member, CommandFlags.FireAndForget);
    }

    public async Task<long> RemoveFromSortedSetAsync(string key, IReadOnlyCollection<string>? members)
    {
        if (members == null || members.Count == 0)
            return 0;
        
        var database = _connection.GetDatabase();
        return await database.SortedSetRemoveAsync(key, members.Select(m => new RedisValue(m)).ToArray());
    }

    public async Task<double?> GetSortedSetScoreAsync(string key, string member)
    {
        var database = _connection.GetDatabase();
        var score = await database.SortedSetScoreAsync(key, member);
        return score;
    }

    public async Task<IReadOnlyCollection<string>> GetSortedSetRangeByScoreAsync(string key, double startScore = double.NegativeInfinity,
        double stopScore = double.PositiveInfinity)
    {
        var database = _connection.GetDatabase();
        var result = await database.SortedSetRangeByScoreAsync(key, startScore, stopScore);

        return result.Select(value => (string)value).ToArray();
    }

    public async Task<long> RemoveSortedSetRangeByScoreAsync(string key, double startScore = double.NegativeInfinity,
        double stopScore = double.PositiveInfinity)
    {
        var database = _connection.GetDatabase();
        return await database.SortedSetRemoveRangeByScoreAsync(key, startScore, stopScore);
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Private

    /// <summary>
    /// Cериализация объекта MessagePack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    RedisValue SerializeMessagePackObject<T>(T value) where T : class, IMessagePackedObject
    {
#if DEBUG && false
                var tempBytes = MessagePackSerializer.Serialize(value, MessagePackOptimizedResolver.StandardOptions);
                var tempJson = MessagePackSerializer.ConvertToJson(tempBytes); //отладочный дамп результата сериализации
#endif
        var bufferWriter = new ArrayBufferWriter<byte>();
        MessagePackSerializer.Serialize(bufferWriter, value, MessagePackOptimizedResolver.StandardOptions);
        RedisValue serialized = bufferWriter.WrittenMemory;
        return serialized;
    }

    /// <summary>
    /// Contractless десериализация объекта MessagePack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    RedisValue SerializeMessagePackContractless<T>(T value)
    {
#if DEBUG && false
                var tempBytes = MessagePackSerializer.Serialize(value, MessagePackOptimizedResolver.ContractlessOptions);
                var tempJson = MessagePackSerializer.ConvertToJson(tempBytes); //отладочный дамп результата сериализации
#endif
        var bufferWriter = new ArrayBufferWriter<byte>();
        MessagePackSerializer.Serialize(bufferWriter, value, MessagePackOptimizedResolver.ContractlessOptions);
        RedisValue serialized = bufferWriter.WrittenMemory;
        return serialized;
    }

    /// <summary>
    /// Десериализация объекта MessagePack.
    /// В случае ошибки десериализации возвращается default.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lease"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    T? DeserializeMessagePackObject<T>(Lease<byte>? lease, string key) where T : class, IMessagePackedObject
    {
#if DEBUG && false
            var tempBytes = (lease != null) ? lease.Memory.ToArray() : new byte[] { };
            var tempJson = MessagePackSerializer.ConvertToJson(tempBytes); //отладочный дамп результата сериализации
#endif
        if (lease == null)
            return default;

        try
        {
            return MessagePackSerializer.Deserialize<T>(lease.Memory, MessagePackOptimizedResolver.StandardOptions);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Redis: load deserialize exception, key - {key}");
            return default;
        }
    }

    /// <summary>
    /// Contractless десериализация объекта MessagePack.
    /// В случае ошибки десериализации возвращается default.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lease"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    T? DeserializeMessagePackContractless<T>(Lease<byte>? lease, string key)
    {
#if DEBUG && false
            var tempBytes = (lease != null) ? lease.Memory.ToArray() : new byte[] { };
            var tempJson = MessagePackSerializer.ConvertToJson(tempBytes); //отладочный дамп результата сериализации
#endif
        if (lease == null) return default;
        try
        {
            return MessagePackSerializer.Deserialize<T>(lease.Memory, MessagePackOptimizedResolver.ContractlessOptions);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Redis: load deserialize exception, key - {key}");
            return default;
        }
    }

    /// <summary>
    /// Contractless десериализация объекта MessagePack.
    /// В случае ошибки десериализации возвращается default.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="redisValue"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    T? DeserializeMessagePackContractless<T>(RedisValue redisValue, string key)
    {
        var bytes = (byte[])redisValue;
#if DEBUG && false
            var tempBytes = bytes?? new byte[] { };
            var tempJson = MessagePackSerializer.ConvertToJson(tempBytes); //отладочный дамп результата сериализации
#endif
        if (bytes == null) return default;
        try
        {
            return MessagePackSerializer.Deserialize<T>(bytes, MessagePackOptimizedResolver.ContractlessOptions);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Redis: load deserialize exception, key - {key}");
            return default;
        }
    }
    #endregion
}