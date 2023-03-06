using AppSample.Domain.DAL;
using AppSample.Infrastructure.DAL;

namespace AppSample.Infrastructure.Repositories;

public class SmsSmppRepository : ISmsSmppRepository
{
    readonly ISmppDAL _smppDal;

    public SmsSmppRepository(ISmppDAL smppDal)
    {
        _smppDal = smppDal;
    }

    public async Task<bool> SendAsync(long ctn, string text)
    {
        return await _smppDal.SendAsync(ctn, text);
    }
}