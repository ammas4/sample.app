using AppSample.CoreTools.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.CoreTools.Exceptions;

public class UnifiedException : Exception
{
	public UnifiedException(OAuth2Error error) : base(error.ToString())
	{
		Error = error;
	}

	public UnifiedException(OAuth2Error error, string? errorDescription)
		: base(error + ": " + errorDescription)
	{
		Error = error;
		ErrorDescription = errorDescription;
	}
	
	public UnifiedException(OAuth2Error error, string? errorDescription, int statusCode)
		: base(error + ": " + errorDescription)
	{
		Error = error;
		ErrorDescription = errorDescription;
		StatusCode = statusCode;
	}

	public OAuth2Error Error { get; }
	public string? ErrorDescription { get; }
	public int? StatusCode { get; set; }
	public int? RetryCount { get; set; }

	public IActionResult FormResponse()
    {
        var response = new ErrorResult(this);
		return new ObjectResult(response) { StatusCode = StatusCode ?? (int)OAuth2ErrorDetails.GetCode(Error) };
	}
}