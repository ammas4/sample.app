namespace AppSample.Domain.Helpers;

/// <summary>
/// Ключи записей кэша в Redis. 
/// Могут использоваться только для значений, которые можно в любое время удалить.
/// Должны начинаться с одного и того же префикса.
/// </summary>
public static class RedisCacheKeys
{
    /// <summary>
    /// Префикс записей кэша
    /// </summary>
    public const string CachePrefix = "Cache:";

    public static string ForRegionOperatorInfo(long msisdn) => $"{CachePrefix}RegionOperatorInfo:{msisdn}";

}