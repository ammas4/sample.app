using System.Net;
using System.Text.Json;
using AppSample.CoreTools.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.CoreTools.Contracts;

public abstract class ExceptionResult
{
    public Exception? Exception { get; set; }

    public bool IsSuccessful => Exception == null;

    public bool IsFailed => Exception != null;

    public IActionResult ToErrorResponse()
    {
        switch (Exception)
        {
            case UnifiedException unifiedException:
            {
                Dictionary<string, string> content = new()
                {
                    [OpenIdConnectParameterNames.Error] = OAuth2ErrorDetails.GetText(unifiedException.Error)
                };

                if (unifiedException.ErrorDescription != null)
                {
                    content.Add(OpenIdConnectParameterNames.ErrorDescription,
                        unifiedException.ErrorDescription);
                }

                if (unifiedException.RetryCount != null)
                {
                    content.Add("retry_count", unifiedException.RetryCount.Value.ToString());
                }

                return new ObjectResult(content)
                {
                    StatusCode = unifiedException.StatusCode ?? (int)OAuth2ErrorDetails.GetCode(unifiedException.Error)
                };
            }
            case TrustedException trustedException:
            {
                if (trustedException.ResponseDict != null)
                    return new ObjectResult(trustedException.ResponseDict)
                    {
                        StatusCode = (int)trustedException.StatusCode
                    };

                if (trustedException.ResponseText != null)
                    return new ObjectResult(trustedException.ResponseText)
                    {
                        StatusCode = (int)trustedException.StatusCode
                    };

                return new StatusCodeResult((int)trustedException.StatusCode);
            }
            case OAuth2Exception oAuth2Exception:
            {
                Dictionary<string, string> content = new()
                {
                    [OpenIdConnectParameterNames.Error] = oAuth2Exception.Error,
                    [OpenIdConnectParameterNames.ErrorDescription] = oAuth2Exception.Description
                };

                return new ObjectResult(content)
                {
                    StatusCode = (int?)oAuth2Exception.StatusCode
                };
            }
            default:
            {
                var content = new Dictionary<string, string>
                {
                    [OpenIdConnectParameterNames.Error] = OAuth2ErrorDetails.GetText(OAuth2Error.ServerError)
                };

                return new ObjectResult(content)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }
    }
}