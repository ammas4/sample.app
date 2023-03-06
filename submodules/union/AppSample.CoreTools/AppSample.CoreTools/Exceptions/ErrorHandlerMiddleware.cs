using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AppSample.CoreTools.Exceptions;

public class ErrorHandlerMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Internal_Server_Error");
            
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            
            Dictionary<string, string> content = new()
            {
                [OpenIdConnectParameterNames.Error] = "Internal_Server_Error",
                [OpenIdConnectParameterNames.ErrorDescription] = "The server encountered an internal error or misconfiguration and was unable to complete your request"
            };

            await response.WriteAsync(JsonSerializer.Serialize(content));
        }
    }
}