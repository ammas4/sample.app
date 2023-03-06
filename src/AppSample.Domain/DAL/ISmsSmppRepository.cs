namespace AppSample.Domain.DAL
{
    public interface ISmsSmppRepository
    {
        Task<bool> SendAsync(long ctn, string text);
    }
}
