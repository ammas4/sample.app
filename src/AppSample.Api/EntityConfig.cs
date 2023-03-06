using AppSample.CoreTools.Settings;
using Microsoft.EntityFrameworkCore;

namespace AppSample.Api;

/// <summary>
/// 
/// </summary>
public static class EntityConfig
{
    public static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var commonConfig = configuration.GetSection(new CommonSettings().SectionName).Get<CommonSettings>();


        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Configure Entity Framework Core to use Microsoft SQL Server.
            options.UseNpgsql(commonConfig.ConnectionString);
        });
    }
}