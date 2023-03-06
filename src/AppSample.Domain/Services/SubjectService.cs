using AppSample.Domain.DAL;
using AppSample.Domain.DAL.Entities;
using AppSample.Domain.Interfaces;

namespace AppSample.Domain.Services
{
    public class SubjectService : ISubjectService
    {
        readonly IDbRepository _dbRepository;

        public SubjectService(IDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }

        public async Task<Guid> GetOrCreateSubject(string msisdn, int serviceProviderId)
        {
            var subject = await _dbRepository.GetSubjectAsync(msisdn, serviceProviderId);

            if (subject == null)
            {
                subject = new SubjectDTO
                {
                    Msisdn = msisdn,
                    ServiceProviderId = serviceProviderId,
                    SubjectId = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _dbRepository.InsertSubjectAsync(subject);
            }

            return subject.SubjectId;
        }

        public async Task DeleteSubjectsForMsisdn(string msisdn)
        {
            await _dbRepository.DeleteSubjectsForMsisdn(msisdn);
        }
    }
}
