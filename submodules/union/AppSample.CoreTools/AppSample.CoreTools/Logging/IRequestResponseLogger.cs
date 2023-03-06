namespace AppSample.CoreTools.Logging;

public interface IRequestResponseLogger
{
    /// <summary>
    /// Запись сессии (запрос/ответ) в логгер
    /// </summary>
    /// <param name="item">сессия (запроса /ответ)</param>
    void Log(LogItem item);
}