using System.Diagnostics;

namespace AppSample.CoreTools.Logging;

public static class LogHelper
{
    /// <summary>
    /// Создание ActivityId
    /// </summary>
    public static Guid CreateActivityId()
    {
        return Guid.NewGuid();
    }

    /// <summary>
    /// Получение текущего ActivityId
    /// </summary>
    public static Guid GetCurrentActivityId()
    {
        SetActivityId();
        return Trace.CorrelationManager.ActivityId;
    }

    /// <summary>
    /// Получение текущего ActivityId или установка нового
    /// </summary>
    public static void SetActivityId()
    {
        if (!Trace.CorrelationManager.ActivityId.Equals(Guid.Empty))
            return;

        Trace.CorrelationManager.ActivityId = CreateActivityId();
    }

    public static string GetClientName(object client)
    {
        string name = client.GetType().Name;
        foreach (var postfix in new[] { "service", "client", "repository" })
        {
            if (name.EndsWith(postfix, StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - postfix.Length);
                break;
            }
        }

        return name;
    }
}