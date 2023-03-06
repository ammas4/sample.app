namespace AppSample.Domain.Models;

/// <summary>
/// Режим авторизации сервис-провайдера
/// </summary>
public enum IdgwAuthMode
{
    /// <summary>
    /// Не выбран
    /// </summary>
    None = 0,

    /// <summary>
    /// SMS с Url
    /// </summary>
    SmsWithUrl = 1,

    /// <summary>
    /// SMS с кодом
    /// </summary>
    SmsOTP = 2,
    
    /// <summary>
    /// USSD
    /// </summary>
    Ussd = 3,
    
    /// <summary>
    /// Seamless
    /// </summary>
    Seamless = 4,
    
    /// <summary>
    /// Пуш в mc-апплет версии 1.0
    /// </summary>
    OldMcPush = 5,
    
    /// <summary>
    /// Пуш в dstk-апплет
    /// </summary>
    DstkPush = 6
}