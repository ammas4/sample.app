using AppSample.Domain.Helpers;
using AppSample.Domain.Models;
using AppSample.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AppSample.Api.Controllers;

[ApiController]
public class JwksController : ControllerBase
{
	readonly SynchronizedMemoryCache _memoryCache;
	readonly IdgwSettings _idgwSettings;
	readonly CacheSettings _cacheSettings;

	public JwksController(
		SynchronizedMemoryCache memoryCache,
		IOptions<IdgwSettings> idgwSettings,
		IOptions<CacheSettings> cacheSettings)
    {
		_memoryCache = memoryCache;
		_idgwSettings = idgwSettings.Value;
		_cacheSettings = cacheSettings.Value;
	}

	[HttpGet("jwks")]
	[Produces("application/json")]
	public async Task<object> Jwks()
	{
		return await _memoryCache.GetOrCreateAsync(CacheKeys.Jwks(), e =>
		{
			e.AbsoluteExpirationRelativeToNow = _cacheSettings.JwksCacheTime;

			MobileConnectJwks jwks = new();

			MobileConnectJwk jwkSig = new(JwksHelper.CreateJwk(_idgwSettings.PrivateKey.Sig, JsonWebKeyUseNames.Sig));
			jwks.Keys.Add(jwkSig);

			MobileConnectJwk jwkEnc = new(new JsonWebKey(_idgwSettings.PublicKey.Enc));
			jwks.Keys.Add(jwkEnc);

			return Task.FromResult(jwks);
		});
	}
}
