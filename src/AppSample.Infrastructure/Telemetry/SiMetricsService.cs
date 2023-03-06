using System.Diagnostics.Metrics;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models.Constants;

namespace AppSample.Infrastructure.Telemetry
{
    /// <summary>
    /// Сервис метрик SI-авторизации
    /// </summary>
    public class SiMetricsService : IBaseMetricsService
	{
		readonly ICachedConfigService _cachedConfigService;

		public SiMetricsService(Meter meter, ICachedConfigService cachedConfigService)
		{
			_cachedConfigService = cachedConfigService;
			RequestCounter = meter.CreateCounter<int>("si-request-counter");
			SmsUrlCounter = meter.CreateCounter<int>("si-request_sms_url-counter");
			SmsOtpCounter = meter.CreateCounter<int>("si-request-sms-otp-counter");
			PushCounter = meter.CreateCounter<int>("si-request-push-counter");
			SeamlessCounter = meter.CreateCounter<int>("si-seamless-counter");
			CompletedCounter = meter.CreateCounter<int>("si-completed-counter");
			DurationHistogram = meter.CreateHistogram<double>("si-duration-histogram", "s", "The duration of SI-requests");
			DiscoveryDurationHistogram = meter.CreateHistogram<double>("si-discovery-duration-histogram", "ms", "The duration of Discovery requests");
			IdgwDurationHistogram = meter.CreateHistogram<double>("si-idgw-duration-histogram", "ms", "The duration of IDGW requests");
			SpCallbackDurationHistogram = meter.CreateHistogram<double>("si-sp-callback-duration-histogram", "ms", "The duration of notifications to service providers");
		}
		
		/// <summary>
		/// Счетчик запросов на авторизацию
		/// </summary>
		protected Counter<int> RequestCounter { get; set; }

		/// <summary>
		/// Счетчик завершенных авторизаций
		/// </summary>
		protected Counter<int> CompletedCounter { get; set; }

		/// <summary>
		/// Счетчик запросов на авторизацию по sms с url
		/// </summary>
		protected Counter<int> SmsUrlCounter { get; set; }

		/// <summary>
		/// Счетчик запросов на авторизацию по sms с otp
		/// </summary>
		protected Counter<int> SmsOtpCounter { get; set; }

		/// <summary>
		/// Счетчик запросов на атворизация по SI Seamless
		/// </summary>
		protected Counter<int> SeamlessCounter { get; set; }

		/// <summary>
		/// Счетчик запросов на авторизацию по push
		/// </summary>
		protected Counter<int> PushCounter { get; set; }

		/// <summary>
		/// Продолжительность авторизации от начала до завершения колбэка сервис-провайдеру
		/// </summary>
		protected Histogram<double> DurationHistogram { get; set; }

		/// <summary>
		/// Продолжительность запроса к Discovery
		/// </summary>
		protected Histogram<double> DiscoveryDurationHistogram { get; set; }

		/// <summary>
		/// Продолжительность запроса к IDGW
		/// </summary>
		protected Histogram<double> IdgwDurationHistogram { get; set; }

		/// <summary>
		/// Продолжительность запроса к сервис-провайдеру
		/// </summary>
		private Histogram<double> SpCallbackDurationHistogram { get; }

		/// <summary>
		/// Добавляет один запрос к счетчику запросов на авторизацию по sms c url
		/// </summary>
		public void AddSmsUrlRequest(string serviceProviderName) =>
			SmsUrlCounter.Add(1, new KeyValuePair<string, object?>(Tags.ServiceProvider, serviceProviderName));

		/// <summary>
		/// Добавляет один запрос к счетчику запросов на авторизацию по sms c otp
		/// </summary>
		public void AddSmsOtpRequest(string serviceProviderName) =>
			SmsOtpCounter.Add(1, new KeyValuePair<string, object?>(Tags.ServiceProvider, serviceProviderName));

		/// <summary>
		/// Добавляет один запрос к счетчику запросов на авторизацию по seamless
		/// </summary>
		public void AddSeamlessRequest(string serviceProviderName) =>
			SeamlessCounter.Add(1, new KeyValuePair<string, object?>(Tags.ServiceProvider, serviceProviderName));

		/// <summary>
		/// Добавляет один запрос к счетчику запросов на авторизацию по push
		/// </summary>
		public void AddPushRequest(string serviceProviderName) =>
			PushCounter.Add(1, new KeyValuePair<string, object?>(Tags.ServiceProvider, serviceProviderName));

		/// <summary>
		/// Добавляет один запрос к счетчику запросов на авторизацию
		/// </summary>
		public void AddRequest(string serviceProviderName) =>
			RequestCounter.Add(1, new KeyValuePair<string, object?>(Tags.ServiceProvider, serviceProviderName));

		/// <summary>
		/// Добавляет продолжительность авторизации
		/// </summary>
		public void AddDuration(DateTimeOffset startDate, string serviceProviderName)
		{
			var tag = new KeyValuePair<string, object?>(Tags.ServiceProvider, serviceProviderName);

			DurationHistogram.Record((DateTimeOffset.Now - startDate).TotalSeconds, tag);
			CompletedCounter.Add(1, tag);
		}

		/// <summary>
		/// Добавляет продолжительность авторизации
		/// </summary>
		public void AddDuration(long elapsedSeconds, string serviceProviderName)
		{
			var tag = new KeyValuePair<string, object?>(Tags.ServiceProvider, serviceProviderName);

			DurationHistogram.Record(elapsedSeconds, tag);
			CompletedCounter.Add(1, tag);
		}
	}
}
