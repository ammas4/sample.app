namespace AppSample.Api.Models;

/// <summary>
/// Входная модель эндпоинта отвта на пуш
/// </summary>
public struct DstkPushAnswerInput
{
    /// <summary>
    /// Номер телефона
    /// </summary>
    public long Msisdn { get; init; }
    
    /// <summary>
    /// Подтвердил ли пользователь
    /// </summary>
    public bool Consent { get; init; }
}