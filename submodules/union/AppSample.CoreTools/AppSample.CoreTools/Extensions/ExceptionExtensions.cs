using System.Net;
using System.Text.Json;
using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.CoreTools.Extensions;

public static class ExceptionExtensions
{
    public static IActionResult ToObjectResult<TResult, TContract>(
        this Result<TResult> result,
        Func<TResult, TContract> mapper)
    {
        return result.Match(
            obj =>
            {
                if (obj is IActionResult resultObj)
                {
                    return resultObj;
                }

                var response = mapper(obj);

                if (obj is IActionResult resultObj2)
                {
                    return resultObj2;
                }

                return new OkObjectResult(response);
            },
            GetErrorResult);
    }

    static IActionResult GetErrorResult(Exception exception)
    {
        switch (exception)
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

                return new ObjectResult(JsonSerializer.Serialize(content))
                {
                    StatusCode = unifiedException.StatusCode ?? (int)OAuth2ErrorDetails.GetCode(unifiedException.Error)
                };
            }
            case TrustedException trustedException:
            {
                if (trustedException.ResponseDict != null)
                    return new ObjectResult(JsonSerializer.Serialize(trustedException.ResponseDict))
                    {
                        StatusCode = (int)trustedException.StatusCode
                    };

                if (trustedException.ResponseText != null)
                    return new ObjectResult(JsonSerializer.Serialize(trustedException.ResponseText))
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

                return new ObjectResult(JsonSerializer.Serialize(content))
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

                return new ObjectResult(JsonSerializer.Serialize(content))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }
    }
}