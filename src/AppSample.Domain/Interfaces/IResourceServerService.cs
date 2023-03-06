using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Interfaces
{
    public interface IResourceServerService
    {
        Task PrepareWarmingInfoAsync(string msisdn, string scope);
        Task IntrospectAuthInfoAsync(AuthorizationRequestDto entity, ServiceProviderEntity serviceProvider, CreateTokensResult tokens);
    }
}
