using AppSample.Domain.Models;
using AppSample.Domain.Services;

namespace AppSample.Domain.Interfaces;

public interface ISIAuthorizeService
{
    Task<SiAuthorizationResult> SiAuthorizeGen(string? clientId, string? scope, string? request, string? responseType);
}