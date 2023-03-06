namespace AppSample.Api.Models;

/// <summary>
/// Тело запроса на подтверждение оплаты
/// </summary>
/// <param name="Code">Уникальный код авторизации</param>
/// <param name="Reason">Результат действия пользователя</param>
public record ConfirmPayRequestVm(
    string? Code,
    string? Reason);