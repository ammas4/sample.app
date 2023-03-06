namespace AppSample.CoreTools.Helpers;

public static class DateTimeHelper
{
    public static DateTime FromUnixTimeSeconds(long seconds)
    {
        var date = new DateTime(DateTime.UnixEpoch.Ticks + seconds * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
        return date;
    }

    static readonly long UnixMinSeconds = DateTime.MinValue.Ticks / TimeSpan.TicksPerSecond - DateTime.UnixEpoch.Ticks / TimeSpan.TicksPerSecond;
    static readonly long UnixMaxSeconds = DateTime.MaxValue.Ticks / TimeSpan.TicksPerSecond - DateTime.UnixEpoch.Ticks / TimeSpan.TicksPerSecond;

    public static bool TryFromUnixTimeSeconds(long seconds, out DateTime date)
    {
        date = default;
        if (seconds < UnixMinSeconds || seconds > UnixMaxSeconds) return false;
        date = new DateTime(DateTime.UnixEpoch.Ticks + seconds * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
        return true;
    }
}