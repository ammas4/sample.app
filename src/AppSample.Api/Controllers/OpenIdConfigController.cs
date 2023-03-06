using AppSample.CoreTools.Settings;
using AppSample.Domain.Helpers;
using AppSample.Domain.Models;
using AppSample.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppSample.Api.Controllers;

[ApiController]
public class OpenIdConfigController : ControllerBase
{
    readonly SynchronizedMemoryCache _memoryCache;
    readonly IdgwSettings _idgwSettings;
    readonly CacheSettings _cacheSettings;

    public OpenIdConfigController(
		SynchronizedMemoryCache memoryCache,
		IOptions<IdgwSettings> idgwSettings,
		IOptions<CacheSettings> cacheSettings)
    {
        _memoryCache = memoryCache;
        _idgwSettings = idgwSettings.Value;
        _cacheSettings = cacheSettings.Value;
    }

	[HttpGet(".well-known/openid-configuration")]
	[Produces("application/json")]
	public async Task<MobileConnectOpenidConfig> OpenIdConfigAggr()
	{
        return await _memoryCache.GetOrCreateAsync(CacheKeys.OpenIdConfig(), async e =>
		{
			e.AbsoluteExpirationRelativeToNow = _cacheSettings.OidcCacheTime;

			var config = new MobileConnectOpenidConfig(_idgwSettings);

			return config;
		});
	}
}
