namespace AppSample.Domain.Interfaces;

public interface ISubjectService
{
    Task<Guid> GetOrCreateSubject(string msisdn, int serviceProviderId);
    Task DeleteSubjectsForMsisdn(string msisdn);
}
