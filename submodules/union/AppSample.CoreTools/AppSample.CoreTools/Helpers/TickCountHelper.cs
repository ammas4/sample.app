namespace AppSample.CoreTools.Helpers;

public static class TickCountHelper
{
    /// <summary>
    /// Время с запуска системы
    /// </summary>
    /// <returns></returns>
    public static TimeSpan GetUpTime()
    {
        return TimeSpan.FromMilliseconds(Environment.TickCount64);
    }
}