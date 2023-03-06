using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using AppSample.CoreTools.Infrustructure.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Interfaces;
using AppSample.CoreTools.Infrustructure;

namespace AppSample.Domain.Services
{
	public class ShortUrlService : IShortUrlService
	{
		readonly ShortUrlSettings _setting;
		readonly IProxyHttpClientFactory _factory;
		readonly ILogger<ShortUrlService> _logger;

		public ShortUrlService(IOptions<ShortUrlSettings> setting, IProxyHttpClientFactory factory, ILogger<ShortUrlService> logger)
		{
			_setting = setting.Value;
			_factory = factory;
			_logger = logger;
		}

		public async Task<string> MinifyUrlAsync(string urlToMinify, string ctn)
		{
			var httpClient = _factory.CreateHttpClient(NamedHttpClient.DefaultProxy, _setting.ServiceUrl, false);
			httpClient.Timeout = _setting!.TimeOut!.Value;

			var content = new CreateLinkRequest() { Url = urlToMinify, Ctn = ctn, Expiration = _setting.ExpirationInDays };
			var json = JsonSerializer.Serialize(content, new JsonSerializerOptions(JsonSerializerDefaults.Web));
			var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

			using var message = new HttpRequestMessage(HttpMethod.Post, _setting.ServiceUrl) { Content = jsonContent };
			message.Headers.Add("api-key", _setting.ApiKey);
			try
			{
				#region logger
				var obj = new
				{
					method = HttpMethod.Post.ToString(),
					uri = _setting.ServiceUrl,
					headers = message.Headers,
					body = json
				};
				_logger.LogInformation($"Short-Url request: {JsonSerializer.Serialize(obj)}");
				#endregion

				var response = await httpClient.SendAsync(message);

				CreateLinkResponse? responseContent = null;
				if (response.StatusCode == HttpStatusCode.OK)
					responseContent = await response.Content.ReadFromJsonAsync<CreateLinkResponse>();

				if (responseContent != null && !string.IsNullOrEmpty(responseContent.ShortUrl))
					return responseContent.ShortUrl!;
				else
					_logger.LogWarning($"Short-Url does not work correctly, please check the service {_setting!.ServiceUrl} or credentials");
			}
			catch (TaskCanceledException ex)
			{
				_logger.LogWarning($"Short-Url request failed by timeout. {ex}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Short-Url request failed");
			}
			return urlToMinify;
		}

		record CreateLinkRequest
		{
			[JsonPropertyName("url")]
			public string? Url { get; set; }

			[JsonPropertyName("expiration")]
			public int? Expiration { get; set; }

			[JsonPropertyName("ctn")]
			public string? Ctn { get; set; }
		}

		class CreateLinkResponse
		{
			[JsonPropertyName("shortUrl")]
			public string? ShortUrl { get; set; }

			[JsonPropertyName("code")]
			public int? Code { get; set; }

			[JsonPropertyName("errorMessage")]
			public string? ErrorMessage { get; set; }

			[JsonPropertyName("errorCode")]
			public int? ErrorCode { get; set; }
		}
	}
}
