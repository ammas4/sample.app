using AppSample.Domain.Models;

namespace AppSample.Domain.Interfaces;

public interface IDiAuthorizeService
{
    Task<DiAuthorizationResult> DiAuthorizeGenAsync(DiAuthorizationCommand command);
    Task<DiCheckConfirmationResult> CheckConfirmationAsync(string sessionId);
    Task<McTokenResult> McTokenAsync(McTokenCommand command);
}