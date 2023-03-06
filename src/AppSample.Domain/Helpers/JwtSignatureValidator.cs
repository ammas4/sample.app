using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AppSample.Domain.Helpers;

public static class JwtSignatureValidator
{
    public static CryptoProviderFactory CryptoProviderFactory = new() { CacheSignatureProviders = false };
    public static bool Validate(string jwtToValidate, string jwks, out IDictionary<string, object>? claims)
    {
        JsonWebKeySet jwksObject = new(jwks);

        foreach (var signKey in jwksObject.GetSigningKeys())
        {
            var tokenValidationResult = new JsonWebTokenHandler().ValidateToken(
                jwtToValidate,
                new()
                {
                    IssuerSigningKey = signKey,
                    CryptoProviderFactory = CryptoProviderFactory,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false
                });

            if (tokenValidationResult.IsValid)
            {
                claims = tokenValidationResult.Claims;
                return true;
            }
        }
        claims = null;
        return false;
	}
}