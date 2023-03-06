using System.Net;

namespace AppSample.CoreTools.Exceptions;

/// <summary>
/// Ошибки, пробрасываемые как есть.
/// Добавлены для ответов от IDGW в SI-авторизации. https://task.mydomain.ru/browse/MOBID-2280
/// </summary>
public class TrustedException : Exception
{
	public TrustedException(HttpStatusCode statusCode, Dictionary<string, object> responseDict)
	{
		StatusCode = statusCode;
		ResponseDict = responseDict;
	}

	public TrustedException(HttpStatusCode statusCode, string? responseText)
	{
		StatusCode = statusCode;
		ResponseText = responseText;
	}

	public HttpStatusCode StatusCode { get; }
	public Dictionary<string, object>? ResponseDict { get; }
	public string? ResponseText { get; }
}