using AppSample.CoreTools.Settings;
using AppSample.Domain.DAL.Entities;
using AppSample.Domain.DAL;
using AppSample.Domain.DAL.DTOs;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Models.Settings;
using AppSample.Infrastructure.DAL.Models;
using AppSample.Infrastructure.Services;

namespace AppSample.Infrastructure.DAL;

public class DbRepository : IDbRepository
{
    readonly string? _connectionString;

    readonly ILogger<DbRepository> _logger;
    readonly IEntityService _entityService;

    const string sqlSelectServiceProviderQuery = @"SELECT id
                    , name
                    , client_id
                    , client_secret
                    , ttl
                    , redirect_urls
                    , notification_urls
                    , redirect_timeout_seconds
                    , auth_mode
                    , otp_notify_url
                    , active
                    , deleted
                    , jwks_content
                    , jwks_url
                    , jwks_enc_content
                    , jwks_enc_url
                    , scopes
                    , doctypes
                    , encryptionmethod
                    , encryption_algorithm
                    , auth_page_url
                    , created_at
                FROM service_providers";

    const string sqlSelectServiceProviderQueryById = @"SELECT id
                        , name           
                        , client_id
                        , client_secret
                        , ttl
                        , redirect_urls
                        , notification_urls
                        , redirect_timeout_seconds
                        , auth_mode
                        , otp_notify_url
                        , active
                        , deleted
                        , jwks_content
                        , jwks_url
                        , jwks_enc_content
                        , jwks_enc_url
                        , scopes
                        , doctypes
                        , encryptionmethod
                        , encryption_algorithm
                        , auth_page_url
                    FROM service_providers WHERE id = @id";

    const string sqlUpdateServiceProviderQuery = @"UPDATE service_providers SET
                                name               = @Name
                               , client_id         = @ClientId
                               , client_secret     = @ClientSecret
                               , ttl               = @TTL
                               , redirect_urls     = @RedirectUrls
                               , notification_urls = @NotificationUrls
                               , updated_at        = @UpdatedAt
                               , auth_mode         = @AuthMode
                               , otp_notify_url    = @OtpNotifyUrl
                               , active            = @Active
                               , deleted           = @Deleted
                               , jwks_content      = @JwksContent
                               , jwks_url          = @JwksUrl
                               , jwks_enc_content  = @JwksEncContent
                               , jwks_enc_url      = @JwksEncUrl
                               , scopes            = @Scopes
                               , doctypes          = @Doctypes
                               , encryptionmethod  = @EncryptionMethod
                               , encryption_algorithm = @EncryptionAlgorithm
                               , auth_page_url     = @AuthPageUrl
                               , redirect_timeout_seconds = @RedirectTimeoutSeconds
                            WHERE id = @Id";

    const string sqlInsertAuthenticatorChainQuery = @"INSERT INTO authenticator_chains
                        (
                              service_provider_id
                            , order_level_1
                            , order_level_2
                            , authenticator_type
                            , next_chain_start_delay
                        )
                        VALUES(
                              @ServiceProviderId
                            , @OrderLevel1
                            , @OrderLevel2
                            , @AuthenticatorType
                            , @NextChainStartDelay
                        )";

    const string sqlUpdateAuthenticatorChainQuery = @"UPDATE authenticator_chains SET
                              order_level_1 = @OrderLevel1
                            , order_level_2 = @OrderLevel2
                            , authenticator_type = @AuthenticatorType
                            , next_chain_start_delay = @NextChainStartDelay
                        WHERE id = @Id AND service_provider_id = @ServiceProviderId";

    const string sqlDeleteAuthenticatorChainQuery = @"DELETE FROM authenticator_chains 
                                        WHERE service_provider_id = @ServiceProviderId";

    NpgsqlConnection Connection =>
        !string.IsNullOrEmpty(_connectionString) ? new NpgsqlConnection(_connectionString) : throw new Exception("Empty connection string");

    public DbRepository(IOptions<CommonSettings> settings, ILogger<DbRepository> logger, IEntityService entityService)
    {
        _connectionString = settings.Value.ConnectionString;
        _logger = logger;
        _entityService = entityService;
    }

    public IEnumerable<SettingsEntity> GetSettings()
    {
        using var connection = Connection;
        var rez = connection.Query<SettingsEntity>("SELECT * FROM settings");
        return rez.ToList();
    }
    
    public async Task<IReadOnlyCollection<SettingsEntity>> GetSettingsAsync()
    {
        await using var connection = Connection;
        var rez = await connection.QueryAsync<SettingsEntity>("SELECT * FROM settings");
        return rez.ToList();
    }

    public async Task SaveSettingsAsync(string name, string? value)
    {
        await using var connection = Connection;

        const string sqlQuery = @"UPDATE Settings SET
                                       value = @value
                                    WHERE name = @name;";

        var updatedRowsCount = await connection.ExecuteAsync(sqlQuery, new { value, name });

        if (updatedRowsCount == 0)
        {
            // Вставить новую запись
            const string insertSqlQuery = @"INSERT INTO Settings (name, value)
	                                        VALUES (@name, @value)
	                                        ON CONFLICT (name) DO NOTHING;";
            var insertedRowsCount = await connection.ExecuteAsync(insertSqlQuery, new { name, value });
        }
    }

    public void InsertEventsList(List<EventDTO> entities)
    {
        throw new NotImplementedException();
    }


    public async Task<AdminUserDTO> LoadAdminUserByIdAsync(int id)
    {
        await using var connection = Connection;
        var rez = await connection.QueryFirstOrDefaultAsync<AdminUserDTO>(
                "SELECT * FROM admin_users WHERE id = @id", new { id }
            );

        return rez;
    }

    public async Task<AdminUserDTO> LoadAdminUserByLoginAsync(string login)
    {
        await using var connection = Connection;
        var rez = await connection.QueryFirstOrDefaultAsync<AdminUserDTO>(
                @"SELECT * FROM Admin_Users WHERE Login=@login",
                new { login }
            );
        return rez;
    }

    public async Task<List<AdminUserDTO>> LoadAllAdminUsersAsync()
    {
        await using var connection = Connection;
        var rez = await connection.QueryAsync<AdminUserDTO>(
                "SELECT * FROM admin_users"
            );

        return rez.ToList();
    }

    public async Task CreateAdminUserAsync(AdminUserDTO entity)
    {
        await using var connection = Connection;
        var sqlQuery = @"INSERT INTO admin_users
	                        (  
		                        login
		                        , role
		                        , active
	                        )
	                        VALUES(
		                        @Login
		                        , @Role
		                        , @Active
	                        )";
        var insertedRowsCount = await connection.ExecuteAsync(sqlQuery, entity);
    }

    public async Task DeleteAdminUserAsync(int id)
    {
        await using var connection = Connection;
        await connection.ExecuteAsync(@"DELETE FROM Admin_Users WHERE Id=@id", new { id });
    }

    #region ServiceProviders

    List<ServiceProviderDb> SelectAllServiceProviders()
    {
        using var connection = Connection;
        var rez = connection.Query<ServiceProviderDb>(sqlSelectServiceProviderQuery);

        return rez.ToList();
    }

    async Task<List<ServiceProviderDb>> SelectAllServiceProvidersAsync()
    {
        await using var connection = Connection;
        var rez = await connection.QueryAsync<ServiceProviderDb>(sqlSelectServiceProviderQuery);

        return rez.ToList();
    }

    public async Task<List<ServiceProviderEntity>> GetAllServiceProvidersAsync(bool limitToSmsOnly)
    {
        var serviceProviders = new List<ServiceProviderEntity>();
        var serviceProvidersDb = await SelectAllServiceProvidersAsync();

        foreach (var serviceProviderDb in serviceProvidersDb)
        {
            var authenticatorsDb = await SelectServiceProviderAuthenticatorsAsync(serviceProviderDb.Id);
            serviceProviders.Add(_entityService.GetServiceProvider(serviceProviderDb, authenticatorsDb, limitToSmsOnly));
        }

        return serviceProviders;
    }

    public List<ServiceProviderEntity> GetAllServiceProviders(bool limitToSmsOnly)
    {
        var serviceProviders = new List<ServiceProviderEntity>();
        var serviceProvidersDb = SelectAllServiceProviders();

        foreach (var serviceProviderDb in serviceProvidersDb)
        {
            var authenticatorsDb = SelectServiceProviderAuthenticators(serviceProviderDb.Id);
            serviceProviders.Add(_entityService.GetServiceProvider(serviceProviderDb, authenticatorsDb, limitToSmsOnly));
        }

        return serviceProviders;
    }

    async Task<ServiceProviderDb> SelectServiceProviderByIdAsync(int id)
    {
        await using var connection = Connection;
        return await connection.QueryFirstOrDefaultAsync<ServiceProviderDb>(sqlSelectServiceProviderQueryById, new { id });
    }

    public async Task<ServiceProviderEntity> GetServiceProviderByIdAsync(int id, bool limitToSmsOnly)
    {
        var serviceProviderDb = await SelectServiceProviderByIdAsync(id);
        var authenticatorsDb = await SelectServiceProviderAuthenticatorsAsync(id);

        var serviceProvider = _entityService.GetServiceProvider(serviceProviderDb, authenticatorsDb, limitToSmsOnly);
        return serviceProvider;
    }

    public async Task CreateServiceProviderAsync(ServiceProviderEntity entity)
    {
        var serviceProviderDb = new ServiceProviderDb(entity);

        await using var connection = Connection;
        const string sqlQuery = @"INSERT INTO service_providers
                        (
                            name             
                            , client_id        
                            , client_secret    
                            , ttl              
                            , redirect_urls    
                            , notification_urls      
							, redirect_timeout_seconds
                            , auth_mode        
                            , otp_notify_url   
							, active           
							, deleted          
							, jwks_content
                            , jwks_url
                            , jwks_enc_content
                            , jwks_enc_url
                            , scopes
                            , doctypes
                            , encryptionmethod
                            , encryption_algorithm
                            , auth_page_url
                        )
                        VALUES(
                            @Name 
                            , @ClientId 
                            , @ClientSecret 
                            , @TTL 
                            , @RedirectUrls 
                            , @NotificationUrls
                            , @RedirectTimeoutSeconds
                            , @AuthMode
                            , @OtpNotifyUrl 
                            , @Active 
                            , @Deleted 
                            , @JwksContent
                            , @JwksUrl
                            , @JwksEncContent
                            , @JwksEncUrl
                            , @Scopes
                            , @Doctypes
                            , @EncryptionMethod
                            , @EncryptionAlgorithm
                            , @AuthPageUrl
						)
                        RETURNING id";
        var id = (int)await connection.ExecuteScalarAsync(sqlQuery, serviceProviderDb);

        foreach (var authenticator in entity.AuthenticatorChain)
        {
            authenticator.ServiceProviderId = id;
            authenticator.Id = default;
            var authenticatorDb = new AuthenticatorDb(authenticator);
            await connection.ExecuteAsync(sqlInsertAuthenticatorChainQuery, authenticatorDb);
        }
    }

    #endregion

    public async Task<List<EventDTO>> GetEventsAsync(DateTime date1, DateTime date2)
    {
        date1 = date1.ToUniversalTime();
        date2 = date2.ToUniversalTime();

        await using var connection = Connection;
        var rez = await connection.QueryAsync<EventDTO>(
            "SELECT * FROM Events WHERE Date>=@date1 AND Date<=@date2 ORDER BY Date,Id",
            new { date1, date2 });
        return rez.ToList();
    }

    public async Task InsertAuthorizationRequestAsync(AuthorizationRequestDto entity)
    {
        await using var connection = Connection;
        const string sqlQuery = @"INSERT INTO authorization_requests
                        (
                            authorization_request_id
                            , notification_uri
                            , notification_token
                            , service_provider_id
                            , scope
                            , acr_values
                            , response_type
                            , msisdn
                            , nonce
                            , correlation_id
                            , consent_code
                            , otp_key
                            , redirect_uri
                            , state
                            , context
                            , binding_message
                            , mode
                            , order_sum
                        )
                        VALUES(
                            @AuthorizationRequestId
                            , @NotificationUri
                            , @NotificationToken
                            , @ServiceProviderId
                            , @Scope
                            , @AcrValues
                            , @ResponseType
                            , @Msisdn
                            , @Nonce
                            , @CorrelationId
                            , @ConsentCode
                            , @OtpKey
                            , @RedirectUri
                            , @State
                            , @Context
                            , @BindingMessage
                            , @Mode
                            , @OrderSum
                        )";
        var insertedRowsCount = await connection.ExecuteAsync(sqlQuery, entity);
    }

    public async Task<IReadOnlyCollection<AuthorizationRequestDto>> GetAuthorizationRequestsAsync(
        IEnumerable<Guid> authReqIds)
    {
        await using var connection = Connection;
        var authRequests = await connection.QueryAsync<AuthorizationRequestDto>(
            "SELECT * FROM Authorization_Requests WHERE authorization_request_id = ANY(@Ids)",
            new { Ids = authReqIds });

        return authRequests.ToArray();
    }

    public async Task<AuthorizationRequestDto?> GetAuthorizationRequestAsync(Guid authReqId)
    {
        await using var connection = Connection;
        var rez = await connection.QueryFirstOrDefaultAsync<AuthorizationRequestDto>(
            "SELECT * FROM Authorization_Requests WHERE authorization_request_id = @id",
            new { id = authReqId });

        return rez;
    }

    public async Task<AuthorizationRequestDto?> GetAuthorizationRequestByConsentCodeAsync(Guid consentCode)
    {
        await using var connection = Connection;
        var rez = await connection.QueryFirstOrDefaultAsync<AuthorizationRequestDto>(
            "SELECT * FROM Authorization_Requests WHERE Consent_Code=@consentCode",
            new { consentCode });

        return rez;
    }

    public async Task<AuthorizationRequestDto?> GetAuthorizationRequestByOtpKeyAsync(Guid otpKey)
    {
        await using var connection = Connection;
        var rez = await connection.QueryFirstOrDefaultAsync<AuthorizationRequestDto>(
            "SELECT * FROM Authorization_Requests WHERE otp_key=@otpKey",
            new { otpKey });

        return rez;
    }

    public async Task<SubjectDTO?> GetSubjectAsync(string msisdn, int serviceProviderId)
    {
        await using var connection = Connection;
        return await connection.QueryFirstOrDefaultAsync<SubjectDTO>(
                "SELECT * FROM subjects WHERE msisdn = @msisdn and service_provider_id = @serviceProviderId",
                new { msisdn, serviceProviderId }
            );
    }

    public async Task InsertSubjectAsync(SubjectDTO entity)
    {
        await using var connection = Connection;
        var sqlQuery = @"INSERT INTO subjects
                        (
                            msisdn
                            , service_provider_id
                            , subject_id
                            , created_at
                        )
                        VALUES(
                            @Msisdn
                            , @ServiceProviderId
                            , @SubjectId
                            , @CreatedAt
                        )";

        await connection.ExecuteAsync(sqlQuery, entity);
    }

    public async Task DeleteSubjectsForMsisdn(string msisdn)
    {
        await using var connection = Connection;
        await connection.ExecuteAsync(@"DELETE FROM subjects WHERE msisdn = @msisdn", new { msisdn });
    }

    public async Task<List<AuthenticatorEntity>> GetServiceProviderAuthenticatorsAsync(int serviceProviderId)
    {
        var authenticators = await SelectServiceProviderAuthenticatorsAsync(serviceProviderId);
        return authenticators.Select(i => EntityService.GetAuthenticatorEntity(i)).ToList();
    }

    async Task<IReadOnlyCollection<AuthenticatorDb>> SelectServiceProviderAuthenticatorsAsync(int serviceProviderId)
    {
        await using var connection = Connection;
        var authenticators = await connection.QueryAsync<AuthenticatorDb>(
            "SELECT * FROM authenticator_chains WHERE service_provider_id = @serviceProviderId",
            new { serviceProviderId });

        return authenticators.ToList();
    }

    IReadOnlyCollection<AuthenticatorDb> SelectServiceProviderAuthenticators(int serviceProviderId)
    {
        using var connection = Connection;
        var authenticators = connection.Query<AuthenticatorDb>(
            "SELECT * FROM authenticator_chains WHERE service_provider_id = @serviceProviderId",
            new { serviceProviderId });

        return authenticators.ToList();
    }
    
    public async Task UpdateServiceProviderAsync(ServiceProviderEntity serviceProvider)
    {
        await using var connection = Connection;
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        {
            await connection.ExecuteAsync(sqlUpdateServiceProviderQuery, new ServiceProviderDb(serviceProvider));

            await connection.ExecuteAsync(
                sqlDeleteAuthenticatorChainQuery,
                new { ServiceProviderId = serviceProvider.Id });

            foreach (var authenticator in serviceProvider.AuthenticatorChain)
            {
                var authenticatorDb = new AuthenticatorDb(authenticator);
                await connection.ExecuteAsync(sqlInsertAuthenticatorChainQuery, authenticatorDb);
            }
            
            await transaction.CommitAsync();
        }
    }
}