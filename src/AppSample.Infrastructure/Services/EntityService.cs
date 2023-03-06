using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Models;
using AppSample.Infrastructure.DAL.Models;
using AppSample.Domain.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AppSample.Infrastructure.Services
{
    public class EntityService : IEntityService
    {
        readonly IdgwSettings _idgwSettings;
        readonly ILogger<EntityService> _logger;
        public EntityService(
            IOptions<IdgwSettings> idgwSettings, 
            ILogger<EntityService> logger)
        {
            _idgwSettings = idgwSettings.Value;
            _logger = logger;
        }

        public ServiceProviderEntity GetServiceProvider(ServiceProviderDb serviceProviderDb, IReadOnlyCollection<AuthenticatorDb>? authenticatorsDb, bool limitToSmsOnly)
        {
            var serviceProvider = GetServiceProvider(serviceProviderDb);

            var authenticators = authenticatorsDb?.Any() == true
                ? authenticatorsDb
                    .Where(a => !limitToSmsOnly || EnumCollections.AvailableForForceSmsAuthenticatorTypes.Contains(a.AuthenticatorType))
                    .Select(GetAuthenticatorEntity)
                    .ToArray()
                : new AuthenticatorEntity[]
                {
                    new()
                    {
                        ServiceProviderId = serviceProvider.Id, 
                        Type = serviceProvider.Type,
                        NextChainStartDelay = TimeSpan.FromSeconds(_idgwSettings.DefaultNextChainStartDelaySeconds)
                    }
                };
            
            serviceProvider.AuthenticatorChain = new AuthenticatorChain(authenticators);
            
            return serviceProvider;
        }

        ServiceProviderEntity GetServiceProvider(ServiceProviderDb db)
        {
            List<Scope>? scopes = ServiceProviderEntity.InitScopes();
            List<DocumentTypes>? documentTypes = null;
            try
            {
                scopes = JsonSerializer.Deserialize<List<Scope>>(db.Scopes, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                documentTypes = JsonSerializer.Deserialize<List<DocumentTypes>>(db.DocTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"failed to deserialize scopes : {db.Scopes}");
            }
            return new ServiceProviderEntity
            {
                Active = db.Active,
                TTL = db.TTL,
                AuthPageUrl = db.AuthPageUrl,
                CreatedAt = db.CreatedAt,
                EncryptionAlgorithm = db.EncryptionAlgorithm,
                UpdatedAt = db.UpdatedAt,
                ClientId = db.ClientId,
                ClientSecret = db.ClientSecret,
                Deleted = db.Deleted,
                EncryptionMethod = db.EncryptionMethod,
                JwksContent = db.JwksContent,
                Id = db.Id,
                JwksEncContent = db.JwksEncContent,
                JwksEncUrl = db.JwksEncUrl,
                JwksUrl = db.JwksUrl,
                Name = db.Name,
                OtpNotifyUrl = db.OtpNotifyUrl,
                RedirectTimeoutSeconds = db.RedirectTimeoutSeconds,
                AuthTypeOld = GetAuthenticatorType(db.AuthMode),

                NotificationUrls = StringsHelper.SplitList(db.NotificationUrls),
                RedirectUrls = StringsHelper.SplitList(db.RedirectUrls),
                Scopes = scopes,
                DocTypes = documentTypes,
            };
        }

        public static AuthenticatorEntity GetAuthenticatorEntity(AuthenticatorDb db) =>
            new()
            {
                Id = db.Id,
                NextChainStartDelay = TimeSpan.FromTicks(db.NextChainStartDelay),
                ServiceProviderId = db.ServiceProviderId,
                OrderLevel1 = db.OrderLevel1,
                OrderLevel2 = db.OrderLevel2,
                Type = db.AuthenticatorType
            };

        public static AuthenticatorType GetAuthenticatorType(IdgwAuthMode db)
        {
            switch( db )
            {
                case IdgwAuthMode.Ussd:
                    return AuthenticatorType.Ussd;
                case IdgwAuthMode.Seamless:
                    return AuthenticatorType.Seamless;
                case IdgwAuthMode.SmsOTP:
                    return AuthenticatorType.SmsOtp;
                case IdgwAuthMode.SmsWithUrl:
                    return AuthenticatorType.SmsWithUrl;
                case IdgwAuthMode.OldMcPush:
                    return AuthenticatorType.PushMc;
                case IdgwAuthMode.DstkPush:
                    return AuthenticatorType.PushDstk;
                case IdgwAuthMode.None:
                    return AuthenticatorType.NoValue;
                default:
                    throw new Exception($"{nameof(IdgwAuthMode)} is not supported");
            }
        }
    }
}
