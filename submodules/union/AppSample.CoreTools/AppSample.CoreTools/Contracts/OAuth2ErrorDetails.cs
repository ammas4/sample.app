using System.Net;
using AppSample.CoreTools.Exceptions;

namespace AppSample.CoreTools.Contracts;

public static class OAuth2ErrorDetails
{
	static readonly Dictionary<OAuth2Error, (string ErrorText, HttpStatusCode StatusCode)> Errors = new()
	{
		[OAuth2Error.AccessDenied] = ("access_denied", HttpStatusCode.Forbidden),
		[OAuth2Error.InvalidGrant] = ("invalid_grant", HttpStatusCode.BadRequest),
		[OAuth2Error.InvalidRequest] = ("invalid_request", HttpStatusCode.BadRequest),
		[OAuth2Error.InvalidScope] = ("invalid_scope", HttpStatusCode.BadRequest),
		[OAuth2Error.ServerError] = ("server_error", HttpStatusCode.InternalServerError),
		[OAuth2Error.TemporarilyUnavailable] = ("temporarily_unavailable", HttpStatusCode.ServiceUnavailable),
		[OAuth2Error.UnauthorizedClient] = ("unauthorized_client", HttpStatusCode.Unauthorized),
		[OAuth2Error.UnsupportedGrantType] = ("unsupported_grant_type", HttpStatusCode.BadRequest),
		[OAuth2Error.UnsupportedResponseType] = ("unsupported_response_type", HttpStatusCode.BadRequest),
		[OAuth2Error.UserInfoError] = ("user_info_error", HttpStatusCode.Forbidden),
		[OAuth2Error.PremiumInfoError] = ("premiuminfo_error", HttpStatusCode.Forbidden),
		[OAuth2Error.Accepted] = ("accepted", HttpStatusCode.Accepted),
		[OAuth2Error.NotFoundEntity] = ("not_found_entity", HttpStatusCode.NotFound),
		[OAuth2Error.Timeout] = ("timeout", HttpStatusCode.RequestTimeout),
		[OAuth2Error.PaymentFail] = ("payment_fail", HttpStatusCode.Forbidden),
		[OAuth2Error.AuthorizationError] = ("authorization_error", HttpStatusCode.Forbidden),
		[OAuth2Error.StatusConflict] = ("status_conflict", HttpStatusCode.Conflict),
	};
	static readonly Dictionary<string, OAuth2Error> Reverse = Errors
		.ToDictionary(p => 
			p.Value.ErrorText, 
			p => p.Key);
	
	static readonly Dictionary<string, string> Synonyms = new()
	{
		["AccessDenied"] = "access_denied",
		["InvalidRequest"] = "invalid_request",
		["Not_Found_Entity"] = "not_found_entity",
	};

	public static HttpStatusCode GetCode(OAuth2Error error) => Errors[error].StatusCode;
	public static string GetText(OAuth2Error error) => Errors[error].ErrorText;

	public static OAuth2Error GetError(string errorText)
	{
		if (Synonyms.ContainsKey(errorText)) 
			errorText = Synonyms[errorText];
		return Reverse.ContainsKey(errorText) ? Reverse[errorText] : OAuth2Error.ServerError;
	}

	public static UnifiedException ReplaceIdgwError(UnifiedException error)
	{
		if (error.ErrorDescription != null)
		{
			switch ((GetText(error.Error) + ":" + error.ErrorDescription).ToLower())
			{
				case "authorization_error:msisdn mismatch in authorization request and operator platform answer":
					return new UnifiedException(OAuth2Error.AccessDenied, "Login hint doesn't match");
				case "authorization_error:seamless authentication failed":
					return new UnifiedException(OAuth2Error.AccessDenied, "HHE request failed");
				case "server_error:hhe unsupported":
					return new UnifiedException(OAuth2Error.AccessDenied, "HHE request failed");
			}
		}

		return error;
	}
}