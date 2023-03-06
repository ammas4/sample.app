namespace AppSample.Api.Models;

/// <summary>
/// Входная модель эндпоинта отвта на пуш
/// </summary>
public struct McPushAnswerInput
{
    /// <summary>
    /// Номер телефона
    /// </summary>
    public long Msisdn { get; init; }

    /// <summary>
    /// Ответ пользователя
    /// </summary>
    public OldMcPushAnswerType Answer { get; init; }
}

/// <summary>
/// Что ответил пользователь
/// </summary>
public enum OldMcPushAnswerType
{
    /// <summary>
    /// Нет значения (если произошла случайная отправка)
    /// </summary>
    NoResult = 0,

    /// <summary>
    /// Нажал Да
    /// </summary>
    UserAgree = 1,

    /// <summary>
    /// Нажал Отмена
    /// </summary>
    UserDenied = 2,
}