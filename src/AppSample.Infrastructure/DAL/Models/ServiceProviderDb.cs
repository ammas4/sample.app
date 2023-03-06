using System.Text.Json;
using AppSample.Domain.Extensions.Models;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Helpers;

namespace AppSample.Infrastructure.DAL.Models;

public struct ServiceProviderDb
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public int? TTL { get; init; }
    public string? RedirectUrls { get; init; }
    public string? NotificationUrls { get; init; }
    public string? AuthPageUrl { get; init; }
    public int? RedirectTimeoutSeconds { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IdgwAuthMode AuthMode { get; init; }
    public string? OtpNotifyUrl { get; init; }


    public bool Active { get; init; }
    public bool Deleted { get; init; }
    public string? JwksContent { get; init; }
    public string? JwksUrl { get; init; }
    public string? JwksEncContent { get; init; }
    public string? JwksEncUrl { get; init; }
    public string Scopes { get; init; }
    public string DocTypes { get; init; }

    public JweMethodEncryption EncryptionMethod { get; init; }

    public JweAlgorithm EncryptionAlgorithm { get; init; }

    public ServiceProviderDb()
    {
        Active = false;
        AuthMode = IdgwAuthMode.None;
        AuthPageUrl = null;
        ClientId = null;
        ClientSecret = null;
        CreatedAt = DateTime.UtcNow;
        Deleted = false;
        EncryptionAlgorithm = JweAlgorithm.RSA_OAEP;
        EncryptionMethod = JweMethodEncryption.A128CBC_HS256;
        Id = 0;
        JwksContent = null;
        JwksEncContent = null;
        JwksEncUrl = null;
        JwksUrl = null;
        Name = null;
        NotificationUrls = null;
        OtpNotifyUrl = null;
        RedirectTimeoutSeconds = null;
        RedirectUrls = null;
        Scopes = string.Empty;
        DocTypes = string.Empty;
        TTL = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public ServiceProviderDb(ServiceProviderEntity entity)
    {
        Active = entity.Active;
        AuthMode = entity.AuthenticatorChain.GetFirst != null ? entity.AuthenticatorChain.GetFirst.Type.GetAuthMode() : IdgwAuthMode.None;
        AuthPageUrl = entity.AuthPageUrl;
        ClientId = entity.ClientId;
        ClientSecret = entity.ClientSecret;
        CreatedAt = entity.CreatedAt;
        Deleted = entity.Deleted;
        EncryptionAlgorithm = entity.EncryptionAlgorithm;
        EncryptionMethod = entity.EncryptionMethod;
        Id = entity.Id;
        JwksContent = entity.JwksContent;
        JwksEncContent = entity.JwksEncContent;
        JwksEncUrl = entity.JwksEncUrl;
        JwksUrl = entity.JwksUrl;
        Name = entity.Name;
        NotificationUrls = StringsHelper.JoinList(entity.NotificationUrls);
        OtpNotifyUrl = entity.OtpNotifyUrl;
        RedirectTimeoutSeconds = entity.RedirectTimeoutSeconds;
        RedirectUrls = StringsHelper.JoinList(entity.RedirectUrls);
        Scopes = JsonSerializer.Serialize(entity.Scopes);
        DocTypes = JsonSerializer.Serialize(entity.DocTypes);
        TTL = entity.TTL;
        UpdatedAt = entity.UpdatedAt;
    }
}