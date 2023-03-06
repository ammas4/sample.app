using Microsoft.AspNetCore.Http;

namespace AppSample.CoreTools.Logging;

public class DefaultHttpLogDataFinder : IHttpLogDataFinder
{
    readonly IHttpContextAccessor _httpContextAccessor;
    public DefaultHttpLogDataFinder(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual Dictionary<string, object?> GetLogDataFromContext(HttpContext? context = null)
    {
        if (context == null)
        {
            context = _httpContextAccessor.HttpContext;
        }

        Dictionary<string, object?> result = new();
        if (context != null)
        {
            var ip = context.GetRemoteIPAddress();
            if (ip != null) result[nameof(ip)] = ip.ToString();
        }

        return result;
    }
}