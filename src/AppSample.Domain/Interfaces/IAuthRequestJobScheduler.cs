using AppSample.Domain.DAL.DTOs;

namespace AppSample.Domain.Interfaces;

public interface IAuthRequestJobScheduler
{
    Func<AuthorizationRequestDto, Task> HandlerAsync { set; }
    
    /// <summary>
    /// Запустить задачу с задержкой
    /// </summary>
    /// <param name="authReqId"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    Task FireWithDelayAsync(Guid authReqId, int seconds);

    /// <summary>
    /// Деактивирует задачу, если она ещё не обработала
    /// </summary>
    /// <param name="authReqId"></param>
    /// <returns>true - удлось деактивировать (таймаут не был обработан),
    /// false - не удалось деактивировать (таймаут уже обработан или обрабатывается сейчас)</returns>
    Task<bool> DeleteJobIfNotProcessedAsync(Guid authReqId);
    Task<bool> IsJobWasProcessed(Guid authReqId);
    Task DeleteJobAsync(Guid authReqId);
}
