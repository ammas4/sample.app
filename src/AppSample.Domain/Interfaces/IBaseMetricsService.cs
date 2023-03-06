namespace AppSample.Domain.Interfaces
{
    public interface IBaseMetricsService
    {
        /// <summary>
        /// Добавляет один запрос к счетчику запросов на авторизацию
        /// </summary>
        void AddRequest(string serviceProviderName);

        /// <summary>
        /// Добавляет один запрос к счетчику запросов на авторизацию по sms c url
        /// </summary>
        void AddSmsUrlRequest(string serviceProviderName);

        /// <summary>
        /// Добавляет один запрос к счетчику запросов на авторизацию по sms с otp
        /// </summary>
        void AddSmsOtpRequest(string serviceProviderName);


        /// <summary>
        /// Добавляет один запрос к счетчику запросов на авторизацию по push
        /// </summary>
        void AddPushRequest(string serviceProviderName);
        
        /// <summary>
        /// Добавляет один запрос к счетчику запросов на авторизацию по seamless
        /// </summary>
        void AddSeamlessRequest(string serviceProviderName);

        /// <summary>
        /// Добавляет продолжительность авторизации
        /// </summary>
        void AddDuration(DateTimeOffset startDate, string serviceProviderName);

        /// <summary>
        /// Добавляет продолжительность авторизации
        /// </summary>
        void AddDuration(long elapsedMilliseconds, string serviceProviderName);
    }
}
