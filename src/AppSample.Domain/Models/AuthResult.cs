namespace AppSample.Domain.Models;

/// <summary>
/// Результат аутентификации
/// </summary>
public enum AuthResult
{
    /// <summary>
    /// Ничего не означает. Эквивалентно null
    /// </summary>
    NoValue = 0,
    /// <summary>
    /// Пользователь ответил согласием
    /// </summary>
    UserAgree = 1,
    /// <summary>
    /// Пользователь ответил отказом
    /// </summary>
    UserDenied = 2,
    /// <summary>
    /// Аутентификация завершилась по таймауту
    /// </summary>
    Timeout = 3,
    /// <summary>
    /// Предыдущая аутентификация ещё не закончена
    /// </summary>
    PreviousNotFinished = 4,
    /// <summary>
    /// Закончились попытки аутентификации
    /// </summary>
    RunOutOfAttempts = 5,
}