using AppSample.Domain.DAL;
using AppSample.Domain.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.CoreTools.Redis;
using Microsoft.Extensions.Logging;

namespace AppSample.Domain.Services;

public class ServiceProviderService : IServiceProviderService
{
    readonly IDbRepository _dbRepository;
    readonly ICachedConfigService _cachedConfigService;
    readonly IRedisService _redisService;
    readonly ILogger<ServiceProviderService> _logger;

    public ServiceProviderService(IDbRepository dbRepository, ICachedConfigService cachedConfigService,
        IRedisService redisService, ILogger<ServiceProviderService> logger)
    {
        _dbRepository = dbRepository;
        _cachedConfigService = cachedConfigService;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<List<ServiceProviderEntity>> GetAllAsync()
    {
        var allSpDtos = await _dbRepository.GetAllServiceProvidersAsync(false);
        var allServiceProdivers = allSpDtos.OrderBy(x => x.Id).ToList();
        return allServiceProdivers;
    }

    public async Task CreateAsync(ServiceProviderEntity item)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        await _dbRepository.CreateServiceProviderAsync(item);
        _cachedConfigService.SignalChange();
    }

    public async Task UpdateAsync(ServiceProviderEntity item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        
        await _dbRepository.UpdateServiceProviderAsync(item);
        _cachedConfigService.SignalChange();
        await _redisService.DeleteAsync(CacheKeys.ServiceProviderJwks(item.Id));
    }

    public async Task DeleteAsync(ServiceProviderEntity item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        item.Deleted = true;
        await _dbRepository.UpdateServiceProviderAsync(item);
        _cachedConfigService.SignalChange();
        await _redisService.DeleteAsync(CacheKeys.ServiceProviderJwks(item.Id));
    }

    public async Task CloneAsync(ServiceProviderEntity item)
    {
        item.Id = default;
        var guid = Guid.NewGuid();
        item.Name += $" Copy-{guid}";
        item.ClientId += $"-copy-{guid}";
        await CreateAsync(item);
    }

    public async Task<ServiceProviderEntity> GetByIdAsync(int id)
    {
        return await _dbRepository.GetServiceProviderByIdAsync(id, false);
    }
}