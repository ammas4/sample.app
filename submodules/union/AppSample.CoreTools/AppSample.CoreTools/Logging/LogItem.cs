using System.Net;

namespace AppSample.CoreTools.Logging;

/// <summary>
/// Сущность логирования полного лога запрос + ответ/ошибка
/// </summary>
public class LogItem 
{
    /// <summary>
    /// Название клиента API
    /// </summary>
    public string? Api { get; set; }

    /// <summary>
    /// Дополнительные данные
    /// </summary>
    public Dictionary<string,object?>? ContextAdditionalData { get; set; }

    /// <summary>
    /// Телефон пользователя
    /// </summary>
    public string? Ctn { get; set; }

    /// <summary>
    /// Тело запроса
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// Заголовки запроса
    /// </summary>
    public Dictionary<string, List<string>>? RequestHeaders { get; set; }

    /// <summary>
    /// Метод запроса
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// Адрес запроса
    /// </summary>
    public string? RequestUrl { get; set; }

    /// <summary>
    /// Тело ответа
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Заголовки ответа
    /// </summary>
    public Dictionary<string, List<string>>? ResponseHeaders { get; set; }

    /// <summary>
    /// Код ответа
    /// </summary>
    public HttpStatusCode ResponseStatusCode { get; set; }

    /// <summary>
    /// Длительность сессии запрос/ответ
    /// </summary>
    public TimeSpan Time { get; set; }

    /// <summary>
    /// IP запроса
    /// </summary>
    public string? IP { get; set; }

    /// <summary>
    /// Текстовый код ошибки
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Числовой код ошибки
    /// </summary>
    public int? ErrorCode { get; set; }

    /// <summary>
    /// Текстовое описание ошибки
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Индикатор ошибки в ответе
    /// </summary>
    public bool? WasError { get; set; }
}