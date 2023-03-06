namespace AppSample.Api.Models;

/// <summary>
/// 
/// </summary>
public readonly struct PushPinAnswerInput
{
    /// <summary>
    /// MSISDN, на который будет отправлен запрос
    /// </summary>
    public long Msisdn { get; init; }

    /// <summary>
    /// Результат пуша с пин-кодом
    /// </summary>
    public PushPinAnswerState VerifyState { get; init; }
}

/// <summary>
/// Статусы выполнения запросов
/// </summary>
public enum PushPinAnswerState
{
    /// <summary>
    /// Значение не было указано
    /// </summary>
    NoValue = 0,

    /// <summary>
    /// Пользователь подтвердил операцию
    /// </summary>
    AcceptedByUser = 1,

    /// <summary>
    /// Пользователь отклонил операцию
    /// </summary>
    DeniedByUser = 2,

    /// <summary>
    /// Пользователь заблокирован
    /// </summary>
    UserBlocked = 3,

    /// <summary>
    /// Время ожидания операции вышло
    /// </summary>
    Timeout = 4,
}