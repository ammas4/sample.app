using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.Api;

public static class RequestExceptionHandler
{
    public static async Task Handle(HttpContext httpContext)
    {
        var error = httpContext.Features.Get<IExceptionHandlerPathFeature>().Error;
        if (error is UnifiedException unifiedException)
        {
            httpContext.Response.StatusCode = unifiedException.StatusCode ?? (int) OAuth2ErrorDetails.GetCode(unifiedException.Error);
            var response = new ErrorResult(unifiedException);
            await httpContext.Response.WriteAsJsonAsync(response);
        }
        else
        {
            httpContext.Response.StatusCode = 500;
            await httpContext.Response.WriteAsJsonAsync(new Dictionary<string, string> { [OpenIdConnectParameterNames.Error] = OAuth2ErrorDetails.GetText(OAuth2Error.ServerError) });
        }
    }
}