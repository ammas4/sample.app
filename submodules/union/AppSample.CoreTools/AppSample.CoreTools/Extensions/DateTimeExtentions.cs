namespace AppSample.CoreTools.Extensions;

public static class DateTimeExtentions
{
    public static long ToUnixTimeSeconds(this DateTime date)
    {
        return new DateTimeOffset(date).ToUnixTimeSeconds();
    }

    public static long ToUnixTimeMilliseconds(this DateTime date)
    {
        return new DateTimeOffset(date).ToUnixTimeMilliseconds();
    }

    public static DateTime TrimToSeconds(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Kind);
    }

    public static DateTime TrimToMinutes(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, dateTime.Kind);
    }

    public static DateTime TrimToHours(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);
    }

}