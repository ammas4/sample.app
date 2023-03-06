using AppSample.Domain.Models;

namespace AppSample.Domain.Interfaces;

public interface IAuthStarter
{
    /// <summary>
    /// Запускает аутентификацию для msisdn через конкретный аутентификатор
    /// </summary>
    /// <param name="startBag"></param>
    /// <returns></returns>
    Task<AuthenticatorStartResult> TryStartAsync(AuthSessionBag startBag);
}