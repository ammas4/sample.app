namespace AppSample.Infrastructure.DAL
{
    public interface ISmppDAL
    {
        Task<bool> SendAsync(long ctn, string text);
    }
}
