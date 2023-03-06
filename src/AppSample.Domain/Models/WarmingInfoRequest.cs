namespace AppSample.Domain.Models
{
    /// <summary>
    /// Модель запроса на прогрев данных Premium Info
    /// </summary>
    public class WarmingInfoRequest
	{
		/// <summary>
		/// Номер телефона абонента
		/// </summary>
		public string Msisdn { get; set; }

		/// <summary>
		/// Запрашиваемые скоупы
		/// </summary>
		public string[] Scopes { get; set; }
	}
}
