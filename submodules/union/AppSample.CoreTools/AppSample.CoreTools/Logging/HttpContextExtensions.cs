using System.Net;
using Microsoft.AspNetCore.Http;

namespace AppSample.CoreTools.Logging;

/// <summary>
/// Расширение для работы с HttpContext
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Получаем IP удаленного клиента
    /// </summary>
    /// <param name="context">Контекст</param>
    /// <param name="allowForwarded">Разрешить проверку заголовка x-forwarded-for</param>
    /// <returns>IPAddress</returns>
    public static IPAddress? GetRemoteIPAddress(this HttpContext context, bool allowForwarded = true)
    {
        if (allowForwarded)
        {
            var ipsFromHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            var ipFromHeader = ipsFromHeader?
                .Split(',')
                .Select(s => s.Trim())
                .FirstOrDefault();

            if (string.IsNullOrEmpty(ipFromHeader) == false && IPEndPoint.TryParse(ipFromHeader, out var ipEndpoint))
                return ipEndpoint.Address;
        }

        return context.Connection.RemoteIpAddress;
    }
}