using OpenTelemetry;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AppSample.CoreTools.ConfigureServices.OpenTelemetry
{
    /// <summary>
    /// Процессор замены в трейсе отображаемое имя SPAN на URL запроса
    /// </summary>
    public class HttpClientSpanNameEnrichingProcessor : BaseProcessor<Activity>
	{
		public override void OnEnd(Activity activity)
		{
			if (activity.DisplayName.StartsWith("HTTP"))
			{
				var httpUrl = activity
					.Tags
					.Where(k => k.Key == "http.url")
					.Select(k => k.Value)
					.FirstOrDefault();

				if (!string.IsNullOrWhiteSpace(httpUrl))
				{
					var uri = new Uri(httpUrl);
					var leftPart = uri.GetLeftPart(UriPartial.Path);
					var noRegexMatches = Regex.Replace(
						leftPart, 
						"[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}", 
						"{guid}");
					activity.DisplayName = noRegexMatches;
				}
			}
		}
	}
}
