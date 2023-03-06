using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Interfaces;

public interface IServiceProviderService
{
    Task<List<ServiceProviderEntity>> GetAllAsync();
    Task CreateAsync(ServiceProviderEntity item);
    Task UpdateAsync(ServiceProviderEntity item);
    Task DeleteAsync(ServiceProviderEntity item);
    Task CloneAsync(ServiceProviderEntity item);
    Task<ServiceProviderEntity> GetByIdAsync(int id);
}