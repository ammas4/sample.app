using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AppSample.CoreTools.Helpers.BatchProcessor;

/// <summary>
/// Класс для пакетной обработки (без возврата результата)
/// </summary>
/// <typeparam name="TItem"></typeparam>
public class BatchProcessor<TItem> : BaseBatchProcessor
{
    readonly ConcurrentQueue<BatchProcessorContainer<TItem>> _containersQueue = new();
    readonly Func<List<TItem>, Task> _processor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger">логгер</param>
    /// <param name="name">название обработчика</param>
    /// <param name="saveTaskPeriod">период срабатывания задачи обработки</param>
    /// <param name="taskCount">число задач обработки</param>
    /// <param name="queueTimeout">лимит времени нахождения в очереди</param>
    /// <param name="maxBatchSize">максимальный размер пакетной обработки</param>
    /// <param name="processor">метод обработки</param>
    public BatchProcessor(ILogger logger, string name, TimeSpan saveTaskPeriod, int taskCount, TimeSpan queueTimeout, int maxBatchSize, Func<List<TItem>, Task> processor)
        : base(logger, name, saveTaskPeriod, taskCount, queueTimeout, maxBatchSize)
    {
        _processor = processor;
    }

    /// <summary>
    /// Планируем обработку
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public Task Schedule(TItem info)
    {
        ThrowIfStopped();

        var taskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var item = new BatchProcessorContainer<TItem>
        {
            Item = info,
            TaskCompletionSource = taskCompletionSource,
            QueueTime = TickCountHelper.GetUpTime()
        };

        _containersQueue.Enqueue(item);

        var task = taskCompletionSource.Task;
        return task;
    }

    /// <summary>
    /// Число ожидающих обработки элементов
    /// </summary>
    public override int Count => _containersQueue.Count;

    /// <summary>
    /// Метод обработки пакета элементов
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    protected override async Task<long> DoProcessItems(int taskId)
    {
        try
        {
            _logger.LogTrace($"{_name}.{taskId} queue size: {_containersQueue.Count}");

            var containers = new List<BatchProcessorContainer<TItem>>();

            while (_containersQueue.TryDequeue(out var container))
            {
                if (TickCountHelper.GetUpTime() - container.QueueTime > _queueTimeout) //проверка времени нахождения элемента в очереди
                {
                    _logger.LogDebug($"{_name}.{taskId} - queue timeout has been reached");
                    container.TaskCompletionSource.SetCanceled();
                    continue;
                }

                containers.Add(container);
                if (containers.Count >= _maxBatchSize)
                {
                    break;
                }
            }

            int containersCount = containers.Count;
            if (containersCount > 0)
            {
                var items = containers.Select(x => x.Item).ToList();
                try
                {
                    await _processor(items);
                }
                catch (Exception exp) //ошибка в методе обработки
                {
                    //выставляем ошибку ожидающим задачам    
                    var exception = new BatchProcessorException("Error in batch processor", exp);
                    containers.ForEach(x => x.TaskCompletionSource.SetException(exception));
                    return containersCount;
                }

                //выставляем результат ожидающим задачам    
                containers.ForEach(x => x.TaskCompletionSource.SetResult());
                return containersCount;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process items");
        }

        return 0;
    }
}

/// <summary>
/// Класс для пакетной обработки (с возвратом результата)
/// </summary>
/// <typeparam name="TItem"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class BatchProcessor<TItem, TResult> : BaseBatchProcessor
{
    readonly ConcurrentQueue<BatchProcessorContainer<TItem, TResult>> _containersQueue = new();
    readonly Func<List<TItem>, Task<List<TResult>>> _processor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger">логгер</param>
    /// <param name="name">название обработчика</param>
    /// <param name="saveTaskPeriod">период срабатывания задачи обработки</param>
    /// <param name="taskCount">число задач обработки</param>
    /// <param name="queueTimeout">лимит времени нахождения в очереди</param>
    /// <param name="maxBatchSize">максимальный размер пакетной обработки</param>
    /// <param name="processor">метод обработки</param>
    public BatchProcessor(ILogger logger, string name, TimeSpan saveTaskPeriod, int taskCount, TimeSpan queueTimeout, int maxBatchSize,
        Func<List<TItem>, Task<List<TResult>>> processor)
        : base(logger, name, saveTaskPeriod, taskCount, queueTimeout, maxBatchSize)
    {
        _processor = processor;
    }

    /// <summary>
    /// Число ожидающих обработки элементов
    /// </summary>
    public override int Count => _containersQueue.Count;

    /// <summary>
    /// Планируем обработку
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public Task<TResult> Schedule(TItem info)
    {
        ThrowIfStopped();

        var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var item = new BatchProcessorContainer<TItem, TResult>
        {
            Item = info,
            TaskCompletionSource = taskCompletionSource,
            QueueTime = TickCountHelper.GetUpTime()
        };

        _containersQueue.Enqueue(item);

        var task = taskCompletionSource.Task;
        return task;
    }

    /// <summary>
    /// Метод обработки пакета элементов
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    protected override async Task<long> DoProcessItems(int taskId)
    {
        try
        {
            _logger.LogTrace($"{_name}.{taskId} queue size: {_containersQueue.Count}");

            var containers = new List<BatchProcessorContainer<TItem, TResult>>();

            while (_containersQueue.TryDequeue(out var container))
            {
                if (TickCountHelper.GetUpTime() - container.QueueTime > _queueTimeout) //проверка времени нахождения элемента в очереди
                {
                    _logger.LogDebug($@"{_name}.{taskId} - queue timeout has been reached");
                    container.TaskCompletionSource.SetCanceled();
                    continue;
                }

                containers.Add(container);
                if (containers.Count >= _maxBatchSize)
                {
                    break;
                }
            }

            int containersCount = containers.Count;
            if (containersCount > 0)
            {
                //данные для обработки
                var items = containers.Select(x => x.Item).ToList();

                List<TResult>? results;
                try
                {
                    results = await _processor(items);
                }
                catch (Exception exp) //ошибка в методе обработки
                {
                    //выставляем ошибку ожидающим задачам    
                    var exception = new BatchProcessorException("Error in batch processor", exp);
                    containers.ForEach(x => x.TaskCompletionSource.SetException(exception));
                    return containersCount;
                }

                if (results == null || results.Count != items.Count) //метод обработки вернул неверное количество результатов
                {
                    //выставляем ошибку ожидающим задачам    
                    var exception = new BatchProcessorException("Error in batch processor - wrong results count");
                    containers.ForEach(x => x.TaskCompletionSource.SetException(exception));
                    return containersCount;
                }

                //выставляем результат ожидающим задачам    
                for (var i = 0; i < containersCount; i++)
                {
                    var container = containers[i];
                    var result = results[i];

                    //выставляем результат
                    container.TaskCompletionSource.SetResult(result);
                }

                return containersCount;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process items");
        }

        return 0;
    }
}