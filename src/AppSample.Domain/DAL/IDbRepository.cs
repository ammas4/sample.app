using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.DAL.Entities;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Models.Settings;

namespace AppSample.Domain.DAL;

public interface IDbRepository
{
    IEnumerable<SettingsEntity> GetSettings();
    Task<IReadOnlyCollection<SettingsEntity>> GetSettingsAsync();
    Task SaveSettingsAsync(string name, string? value);
    
    void InsertEventsList(List<EventDTO> entities);

    Task<List<AdminUserDTO>> LoadAllAdminUsersAsync();
    Task CreateAdminUserAsync(AdminUserDTO entity);

    Task<AdminUserDTO> LoadAdminUserByLoginAsync(string login);
    Task<AdminUserDTO> LoadAdminUserByIdAsync(int id);
    Task DeleteAdminUserAsync(int id);


    List<ServiceProviderEntity> GetAllServiceProviders(bool limitToSmsOnly);
    Task<List<ServiceProviderEntity>> GetAllServiceProvidersAsync(bool limitToSmsOnly);
    Task<ServiceProviderEntity> GetServiceProviderByIdAsync(int id, bool limitToSmsOnly);
    Task CreateServiceProviderAsync(ServiceProviderEntity entity);
    Task UpdateServiceProviderAsync(ServiceProviderEntity entity);
    
    Task<List<AuthenticatorEntity>> GetServiceProviderAuthenticatorsAsync(int serviceProviderId);

    Task<List<EventDTO>> GetEventsAsync(DateTime date1, DateTime date2);

    Task InsertAuthorizationRequestAsync(AuthorizationRequestDto entity);
    Task<AuthorizationRequestDto?> GetAuthorizationRequestByConsentCodeAsync(Guid consentCode);
    Task<AuthorizationRequestDto?> GetAuthorizationRequestByOtpKeyAsync(Guid otpKey);
    Task<AuthorizationRequestDto?> GetAuthorizationRequestAsync(Guid authReqId);
    Task<IReadOnlyCollection<AuthorizationRequestDto>> GetAuthorizationRequestsAsync(IEnumerable<Guid> authReqIds);

    Task<SubjectDTO?> GetSubjectAsync(string msisdn, int serviceProviderId);
    Task InsertSubjectAsync(SubjectDTO entity);
    Task DeleteSubjectsForMsisdn(string msisdn);
}

