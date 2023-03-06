using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Services.AuthenticationChain;

namespace AppSample.Domain.Interfaces;

public interface IAuthenticationResultHandler
{
    /// <summary>
    /// Обрабатывает результат, который вернул аутентификатор после выполнения.
    /// </summary>
    /// <param name="handleResultBag"></param>
    /// <param name="authenticatorType">Какой аутентификатор обработал</param>
    /// <param name="authResult"></param>
    /// <returns></returns>
    Task HandleResultAsync(AuthSessionBag handleResultBag, AuthenticatorType authenticatorType, AuthResult authResult);
}