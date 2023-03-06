using Hangfire.Dashboard;

namespace AppSample.Admin.Helpers;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}