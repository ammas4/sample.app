using AppSample.CoreTools.Redis;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppSample.Domain.Services.AuthenticationChain;

public interface IAuthChainJobScheduler : IAuthRequestJobScheduler
{
}

public class AuthChainJobScheduler : AuthRequestJobScheduler, IAuthChainJobScheduler
{
    public AuthChainJobScheduler(IRedisService redisService, IDbRepository dbRepository,
        ILogger<AuthRequestJobScheduler> logger) : base("chain", redisService, dbRepository, logger)
    {
    }
}