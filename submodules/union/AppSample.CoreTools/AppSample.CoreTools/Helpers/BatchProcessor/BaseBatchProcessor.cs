using System.Diagnostics;
using Microsoft.Extensions.Logging;
using static System.FormattableString;

namespace AppSample.CoreTools.Helpers.BatchProcessor;

/// <summary>
/// Базовый класс для пакетной обработки
/// </summary>
public abstract class BaseBatchProcessor
{
    protected readonly ILogger _logger;
    protected readonly TimeSpan _saveTaskPeriod;
    protected readonly List<Task> _saveTasks = new();
    protected readonly TimeSpan _queueTimeout;
    protected readonly int _taskCount;
    protected readonly int _maxBatchSize;
    protected volatile bool _stopped;
    protected readonly string _name;

    /// <summary>
    /// Число ожидающих обработки элементов
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger">логгер</param>
    /// <param name="name">название обработчика</param>
    /// <param name="saveTaskPeriod">период срабатывания задачи обработки</param>
    /// <param name="taskCount">число задач обработки</param>
    /// <param name="queueTimeout">лимит времени нахождения в очереди</param>
    /// <param name="maxBatchSize">максимальный размер пакетной обработки</param>
    protected BaseBatchProcessor(ILogger logger, string name, TimeSpan saveTaskPeriod, int taskCount, TimeSpan queueTimeout, int maxBatchSize)
    {
        _logger = logger;
        if (string.IsNullOrEmpty(name))
            _name = Guid.NewGuid().ToString();
        else
            _name = name;
        if (saveTaskPeriod <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(saveTaskPeriod));
        _saveTaskPeriod = saveTaskPeriod;
        if (taskCount < 1) throw new ArgumentOutOfRangeException(nameof(taskCount));
        _taskCount = taskCount;
        if (queueTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(queueTimeout));
        _queueTimeout = queueTimeout;
        if (maxBatchSize < 1) throw new ArgumentOutOfRangeException(nameof(maxBatchSize));
        _maxBatchSize = maxBatchSize;
    }

    /// <summary>
    /// Запуск обработчика
    /// </summary>
    public void Start()
    {
        for (var i = 0; i < _taskCount; i++)
        {
            var taskId = i;
            _saveTasks.Add(Task.Factory.StartNew(async () => await RunProcessItemsTask(taskId), TaskCreationOptions.LongRunning).Unwrap());
        }
    }

    /// <summary>
    /// Остановка обработчика
    /// </summary>
    public void Stop()
    {
        _stopped = true;
        Task.WaitAll(_saveTasks.ToArray());
    }

    /// <summary>
    /// Проверка на остановку
    /// </summary>
    protected void ThrowIfStopped()
    {
        if (_stopped) throw new BatchProcessorException($"Batch processor {_name} was already stopped");
    }

    /// <summary>
    /// Метод для задачи обработки
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    async Task RunProcessItemsTask(int taskId)
    {
        _logger.LogDebug(Invariant($"Processing task {_name}.{taskId} started: max batch size {_maxBatchSize}, period {_saveTaskPeriod.TotalSeconds} second(s)"));

        TimeSpan? lastSaveTime = null;
        while (_stopped == false || Count > 0)
        {
            var delay = _saveTaskPeriod;
            if (lastSaveTime.HasValue)
            {
                delay -= lastSaveTime.Value; //время задержки уменьшаем на последнее время выполнения
            }

            if (_stopped == false && Count < _maxBatchSize && delay >= TimeSpan.FromMilliseconds(15)) 
            {
                await Task.Delay(delay);
            }

            if (Count > 0)
            {
                var stopwatch = Stopwatch.StartNew();
                var count = await DoProcessItems(taskId);
                stopwatch.Stop();
                lastSaveTime = stopwatch.Elapsed;
                _logger.LogDebug($"Process {count} items call duration: {lastSaveTime.Value.TotalSeconds} second(s)");
            }
            else
            {
                lastSaveTime = null;
            }
        }

        _logger.LogDebug($"Processing task {_name}.{taskId} task completed");
    }

    /// <summary>
    /// Метод обработки пакета элементов
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    protected abstract Task<long> DoProcessItems(int taskId);
}