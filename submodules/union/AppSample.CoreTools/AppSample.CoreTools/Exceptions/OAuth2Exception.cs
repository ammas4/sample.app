using System.Net;

namespace AppSample.CoreTools.Exceptions;

public class OAuth2Exception : Exception
{
    public string Error { get; }
    public string Description { get; }
    public HttpStatusCode StatusCode { get; }

    public OAuth2Exception(string error, string description, HttpStatusCode statusCode)
        : base(error)
    {
        Error = error;
        Description = description;
        StatusCode = statusCode;
    }

    public static OAuth2Exception ParameterMissing => new(
        "Invalid_Request",
        "Parameter missing",
        HttpStatusCode.BadRequest);
    
    public static OAuth2Exception InvalidRedirectUrl => new(
        "Invalid_Request",
        "Invalid Redirect_URL",
        HttpStatusCode.BadRequest);
    
    public static OAuth2Exception YourRequestNeedsAValidSessionOrValidCredentials => new(
        "Unauthorized_Access",
        "Your request needs a valid session or valid credentials",
        HttpStatusCode.Unauthorized);
    
    public static OAuth2Exception UnexpectedError => new(
        "Internal_Server_Error",
        "The server encountered an internal error or misconfiguration and was unable to complete your request",
        HttpStatusCode.InternalServerError);
}