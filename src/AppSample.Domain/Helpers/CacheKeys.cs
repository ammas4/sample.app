namespace AppSample.Domain.Helpers;

public class CacheKeys
{
    /// <summary>
    /// Openid config
    /// </summary>
    public static string OpenIdConfig() => "openid.config";

    /// <summary>
    /// JWKS
    /// </summary>
    public static string Jwks() => "jwks";

    /// <summary>
    /// JWKS сервис-провайдера
    /// </summary>
    public static string ServiceProviderJwks(int serviceProviderId) => $"sp.jwks.{serviceProviderId}";
}