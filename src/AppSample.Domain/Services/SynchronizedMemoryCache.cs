using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace AppSample.Domain.Services
{
    /// <summary>
    /// Используется для избежания параллельного выполнения делегатов создания значения в кэше,
    /// когда значение в кэше отсутствует и запрашивается одновременно в нескольких потоках
    /// </summary>
    public class SynchronizedMemoryCache
	{
		static readonly ConcurrentDictionary<object, SemaphoreSlim> SemaphoresByCacheKeys = new();
		readonly IMemoryCache _memoryCache;

		public SynchronizedMemoryCache(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		/// <summary>
		/// Используется в ситуации, когда есть вложенное кэширование,
		/// чтобы период внешнего кэширования не зависел от внутренного
		/// </summary>
		public async Task<TItem> GetOrCreateAsync<TItem>(
			object key, Func<Task<TItem>> factory, TimeSpan absoluteExpirationRelativeToNow)
		{
			var semaphore = GetSemaphore(key);

            if (!_memoryCache.TryGetValue(key, out TItem result))
            {
                try
                {
                    await semaphore.WaitAsync();
                    if (!_memoryCache.TryGetValue(key, out result))
                    {
                        result = await factory();
                        _memoryCache.Set(key, result, absoluteExpirationRelativeToNow);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }

            return result;
		}

		public async Task<TItem> GetOrCreateAsync<TItem>(object key, Func<ICacheEntry, Task<TItem>> factory)
		{
			var semaphore = GetSemaphore(key);

            if (!_memoryCache.TryGetValue(key, out TItem result))
            {
                try
                {
                    await semaphore.WaitAsync();
                    result = await _memoryCache.GetOrCreateAsync(key, factory);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            return result;
		}

		static SemaphoreSlim GetSemaphore(object key)
		{
            return SemaphoresByCacheKeys.GetOrAdd(key, key => new SemaphoreSlim(1, 1));
		}
	}
}
