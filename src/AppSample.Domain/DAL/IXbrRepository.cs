namespace AppSample.Domain.DAL;

public interface IXbrRepository
{
    Task<string?> GetMsisdnFromXbrTokenAsync(string xbrToken);
}
