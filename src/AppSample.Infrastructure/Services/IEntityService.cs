using AppSample.Domain.Models.ServiceProviders;
using AppSample.Infrastructure.DAL.Models;

namespace AppSample.Infrastructure.Services
{
    public interface IEntityService
    {
        ServiceProviderEntity GetServiceProvider(ServiceProviderDb serviceProviderDb, IReadOnlyCollection<AuthenticatorDb>? authenticatorsDb, bool limitToSmsOnly);
    }
}
