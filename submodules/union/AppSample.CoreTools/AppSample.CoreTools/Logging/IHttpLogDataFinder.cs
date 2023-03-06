using Microsoft.AspNetCore.Http;

namespace AppSample.CoreTools.Logging;

/// <summary>
/// Получение дополнительных данных для логирования из HttpContext
/// </summary>
public interface IHttpLogDataFinder
{
    Dictionary<string, object?> GetLogDataFromContext(HttpContext? context = null);
}