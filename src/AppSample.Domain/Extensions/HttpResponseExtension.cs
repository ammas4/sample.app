using Microsoft.AspNetCore.Http;

namespace AppSample.Domain.Extensions;

public static class HttpResponseExtension
{
    public static void SetCookie(this HttpResponse response, string cookieKey, string cookieValue, bool httpOnly)
    {
        response.Cookies.Delete(cookieKey);
        response.Cookies.Append(cookieKey, cookieValue, new CookieOptions { HttpOnly = httpOnly });
    }
}