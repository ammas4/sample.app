using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Interfaces;

public interface IDiAuthStateService
{
    Task InitAsync(Guid sessionId);

    Task ProcessTimeoutAsync(Guid sessionId);

    Task ProcessRejectAsync(Guid sessionId);

    Task ProcessConfirmAsync(Guid sessionId, AuthenticatorType authenticatorType);

    Task<DiStatusInfo> GetStatusAsync(Guid sessionId);
    Task<(Guid?, AuthenticatorType)> GetAuthIdByOneTimeCodeAsync(string code);
}