using AppSample.CoreTools.Redis;
using AppSample.Domain.DAL;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppSample.Domain.Services;

public abstract class AuthRequestJobScheduler : IAuthRequestJobScheduler
{
    readonly IDbRepository _dbRepository;
    readonly ILogger<AuthRequestJobScheduler> _logger;
    readonly ProcessedJob _processedJob;
    readonly ActiveJob _activeJob;
    readonly JobLocks _jobLocks;

    protected AuthRequestJobScheduler(
        string keyPrefix,
        IRedisService redisService,
        IDbRepository dbRepository,
        ILogger<AuthRequestJobScheduler> logger
    )
    {
        _dbRepository = dbRepository;
        _logger = logger;

        _processedJob = new ProcessedJob(redisService, keyPrefix);
        _activeJob = new ActiveJob(redisService, keyPrefix);
        _jobLocks = new JobLocks(redisService, TimeSpan.FromSeconds(10), keyPrefix);

        _ = RunAsync(CancellationToken.None);
    }

    public async Task FireWithDelayAsync(Guid authReqId, int seconds) =>
        await _activeJob.AddAsync(authReqId, seconds);

    public async Task<bool> DeleteJobIfNotProcessedAsync(Guid authReqId)
    {
        if (!await _jobLocks.TryLockAsync(authReqId))
            return true;

        if (await _processedJob.HasValueAsync(authReqId))
            return true;

        await _activeJob.RemoveAsync(authReqId);

        return false;
    }

    public async Task DeleteJobAsync(Guid authReqId) => await _activeJob.RemoveAsync(authReqId);

    public async Task<bool> IsJobWasProcessed(Guid authReqId) => await _processedJob.HasValueAsync(authReqId);

    public Func<AuthorizationRequestDto, Task>? HandlerAsync { set; get; }

    async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await HandleJobAsync();
            await Task.Delay(1_000, cancellationToken);
        }
    }

    async Task HandleJobAsync()
    {
        try
        {
            await _activeJob.RemoveOldAsync(TimeSpan.FromSeconds(600));
            await _processedJob.DeleteOldAsync(TimeSpan.FromSeconds(600));

            var authReqIds = await _activeJob.GetExpiredAsync();
            if (authReqIds.Count == 0)
                return;

            var authRequests = await _dbRepository.GetAuthorizationRequestsAsync(authReqIds);

            foreach (var authRequest in authRequests)
            {
                var authReqId = authRequest.AuthorizationRequestId;

                if (!await _jobLocks.TryLockAsync(authReqId))
                    continue;

                if (HandlerAsync == null)
                    throw new InvalidOperationException(
                        $"{nameof(AuthRequestJobScheduler)}.{nameof(HandleJobAsync)}: {nameof(HandlerAsync)} == null");

                await _activeJob.RemoveAsync(authReqId);
                await _processedJob.AddAsync(authReqId);
                await _jobLocks.ReleaseAsync(authReqId);

                var handlerTask = HandlerAsync(authRequest);
                var _ = handlerTask
                    .ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnRanToCompletion)
                    .ContinueWith(_ => { }, TaskContinuationOptions.NotOnRanToCompletion);
            }
        }
        // перехватываю все исключения, т.к. Timer кладёт ThreadPool, а тот кладёт процесс
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(AuthRequestJobScheduler)}.{nameof(HandleJobAsync)}");
        }
    }

    class ActiveJob
    {
        readonly IRedisService _redisService;
        readonly string _keyPrefix;

        public ActiveJob(IRedisService redisService, string keyPrefix)
        {
            _redisService = redisService;
            _keyPrefix = keyPrefix;
        }

        string AuthRequestJobKey => $"{_keyPrefix}-auth-req-active-jobs-sorted-set-v1";

        public async Task AddAsync(Guid authReqId, int seconds)
        {
            var jobsTicks = DateTime.UtcNow.AddSeconds(seconds).Ticks;
            await _redisService.AddToSortedSetAsync(AuthRequestJobKey, authReqId.ToString(), jobsTicks);
        }

        public async Task RemoveAsync(Guid authReqId)
        {
            await _redisService.RemoveFromSortedSetAsync(AuthRequestJobKey, authReqId.ToString());
        }

        /// <summary>
        /// Подчищаю таймауты старше 10 минут.
        /// Т.е. в редисе остались authReqId, которые не были удалены при обработке (нет в базе, не ушёл Notify)
        /// </summary>
        /// <param name="timeSpan"></param>
        public async Task RemoveOldAsync(TimeSpan timeSpan)
        {
            await _redisService.RemoveSortedSetRangeByScoreAsync(AuthRequestJobKey,
                stopScore: DateTime.UtcNow.Ticks - timeSpan.Ticks);
        }

        public async Task<IReadOnlyCollection<Guid>> GetExpiredAsync()
        {
            var utcNowTicks = DateTime.UtcNow.Ticks;
            var authReqIdStrings =
                await _redisService.GetSortedSetRangeByScoreAsync(AuthRequestJobKey, stopScore: utcNowTicks);

            if (authReqIdStrings.Count == 0)
                return Enumerable.Empty<Guid>().ToArray();

            var authReqIds = authReqIdStrings
                .Select(idStr => Guid.TryParse(idStr, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty);

            return authReqIds.ToArray();
        }
    }

    class ProcessedJob
    {
        readonly IRedisService _redisService;
        readonly string _keyPrefix;

        public ProcessedJob(IRedisService redisService, string keyPrefix)
        {
            _redisService = redisService;
            _keyPrefix = keyPrefix;
        }

        string JobsRedisKey => $"{_keyPrefix}-auth-req-processed-jobs-sorted-set-v1";

        /// <summary>
        /// Проверяю отдельный список уже отработавших таймаутов (чёрный список). Чтобы не блокировать вызывающую операцию,
        /// если будет рассинхрон в кластере редиса
        /// </summary>
        /// <param name="authReqId"></param>
        /// <returns></returns>
        public async Task<bool> HasValueAsync(Guid authReqId)
        {
            var ticks = await _redisService.GetSortedSetScoreAsync(JobsRedisKey, authReqId.ToString());
            return ticks.HasValue;
        }

        public async Task AddAsync(Guid authReqId)
        {
            await _redisService.AddToSortedSetAsync(JobsRedisKey, authReqId.ToString(), DateTime.UtcNow.Ticks);
        }

        public async Task DeleteOldAsync(TimeSpan delay)
        {
            await _redisService.RemoveSortedSetRangeByScoreAsync(JobsRedisKey,
                stopScore: DateTime.UtcNow.Ticks - delay.Ticks);
        }
    }

    class JobLocks
    {
        readonly TimeSpan _lockTtl;
        readonly IRedisService _redisService;
        readonly string _keyPrefix;

        public JobLocks(IRedisService redisService, TimeSpan ttl, string keyPrefix)
        {
            _redisService = redisService;
            _lockTtl = ttl;
            _keyPrefix = keyPrefix;
        }

        const string IsLocked = "is-locked";

        public async Task<bool> TryLockAsync(Guid authReqId)
        {
            var stateKey = JobStateKey(authReqId);

            // 1. Нужен, чтобы не освежать ttl вызовом GetAndSetStringAsync.
            // 2. Если выставить просто IsExist, то может получиться вечный ключ в следующем сценарии:
            // - GetAndSetStringAsync не атомарно устанавливает значение и ttl
            // - после установки значения текущий процесс завершается, не выставив ttl
            // - другой процесс не может пройти через IsExist, чтобы выставить ttl
            if (await _redisService.KeyTimeToLiveAsync(stateKey) != null)
                return false;

            var isFirst = await _redisService.GetAndSetStringAsync(stateKey, IsLocked, _lockTtl) != IsLocked;
            return isFirst;
        }

        public async Task ReleaseAsync(Guid authReqId)
        {
            await _redisService.DeleteAsync(JobStateKey(authReqId));
        }

        string JobStateKey(Guid authReqId) => $"{_keyPrefix}-auth-req-job-lock-v1:{authReqId}";
    }
}