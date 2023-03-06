namespace AppSample.Domain.Interfaces
{
    public interface IShortUrlService
    {
        Task<string> MinifyUrlAsync(string urlToMinify, string ctn);
    }
}
