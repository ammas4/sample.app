namespace AppSample.CoreTools.Infrustructure.Interfaces
{
    /// <summary>
    /// Фабрика создания HttpClient  с учетом прокси
    /// </summary>
    public interface IProxyHttpClientFactory
    {
        /// <summary>
        /// Создает HttpClient без прокси, если host, к которому строится запрос, в списке исключений.
        /// Иначе создается именованный.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="allowUntrustedSsl"></param>
        /// <returns></returns>
        HttpClient CreateHttpClient(string name, string? url, bool allowUntrustedSsl);
    }
}
