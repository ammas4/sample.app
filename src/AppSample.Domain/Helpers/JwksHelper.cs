using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace AppSample.Domain.Helpers;

public static class JwksHelper
{
    public static JsonWebKey CreateJwk(string privateKey, string use)
    {
        if (!new string[] { JsonWebKeyUseNames.Sig, JsonWebKeyUseNames.Enc }.Any(u => u == use))
            throw new Exception("Wrong jwk use");

        using var rsa = RSA.Create();

        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(new(rsa));

        jwk.Alg = SecurityAlgorithms.RsaSha256;
        jwk.Kid = HashHelper.HashStringFromUtf8(privateKey);
        jwk.Use = use;

        return jwk;
    }
}