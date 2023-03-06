namespace AppSample.CoreTools.Infrustructure
{
    public static class NamedHttpClient
    {
        /// <summary>
        /// HttpClient. Игнорировать проблемы с SSL-сертификатом Сервис-Провайдера. Прокси по-умолчанию
        /// </summary>
        public const string AllowedUntrustedSsl = "AllowedUntrustedSsl";

        /// <summary>
        /// HttpClient. Игнорировать проблемы с SSL-сертификатом Сервис-Провайдера. Не использовать прокси
        /// </summary>
        public const string AllowedUntrustedSslNoProxy = "AllowedUntrustedSslNoProxy";

        /// <summary>
        /// HttpClient. Не использовать прокси
        /// </summary>
        public const string NoProxy = "NoProxy";

        /// <summary>
        /// HttpClient. Прокси по-умолчанию
        /// </summary>
        public const string DefaultProxy = "DefaultProxy";

        /// <summary>
        /// HttpClient. Прокси для Discovery
        /// </summary>
        public const string DiscoveryProxy = "DiscoveryProxy";

        /// <summary>
        /// HttpClient. Прокси для IDGW
        /// </summary>
        public const string IdgwProxy = "IdgwProxy";
    }
}
