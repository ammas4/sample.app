namespace AppSample.CoreTools.Helpers.BatchProcessor;

/// <summary>
/// Класс для хранения данных для пакетной обработки (без возврата результата)
/// </summary>
/// <typeparam name="TItem"></typeparam>
public class BatchProcessorContainer<TItem>
{
    public TaskCompletionSource TaskCompletionSource { get; set; }

    /// <summary>
    /// Элемент для обработки
    /// </summary>
    public TItem Item { get; set; }

    /// <summary>
    /// Время (с момент запуска системы) постановки в очередь
    /// </summary>
    public TimeSpan QueueTime { get; set; }
}

/// <summary>
/// Класс для хранения данных для пакетной обработки (c возвратом результата)
/// </summary>
/// <typeparam name="TItem"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class BatchProcessorContainer<TItem, TResult>
{
    public TaskCompletionSource<TResult> TaskCompletionSource { get; set; }

    /// <summary>
    /// Элемент для обработки
    /// </summary>
    public TItem Item { get; set; }

    /// <summary>
    /// Время (с момент запуска системы) постановки в очередь
    /// </summary>
    public TimeSpan QueueTime { get; set; }
}