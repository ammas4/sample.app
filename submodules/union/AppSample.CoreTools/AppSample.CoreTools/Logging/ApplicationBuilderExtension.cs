using Microsoft.AspNetCore.Builder;

namespace AppSample.CoreTools.Logging;

public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<NLogMiddleware>();
    }
}