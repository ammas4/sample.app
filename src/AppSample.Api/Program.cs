using AppSample.CoreTools.Logging;
using AppSample.CoreTools.Settings;
using AppSample.Api;
using AppSample.Domain.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NLog.Web;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Host.UseNLog();

    // set Intelab.SMPP license
    Inetlab.SMPP.LicenseManager.SetLicense(builder.Configuration["SmppSettings:InetlabLicense"]);

    // Add services to the container.

    ServicesConfig.Configure(builder.Services, builder.Configuration, false);
    EntityConfig.Configure(builder.Services, builder.Configuration);

    builder.Services.AddControllersWithViews();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() {Title = "AppSample api", Version = "v1"});
        c.AddSecurityDefinition("basic", new()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "basic",
            In = ParameterLocation.Header,
            Description = "Basic Authorization header using the Bearer scheme."
        });

        c.AddSecurityRequirement(new()
        {
            {
                new()
                {
                    Reference = new()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "basic"
                    }
                },
                new string[] { }
            }
        });
        c.AddSecurityDefinition("Bearer",
            new()
            {
                In = ParameterLocation.Header,
                Description = "Please enter into field the word 'Bearer' following by space and JWT",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
        c.AddSecurityRequirement(new()
        {
            {
                new()
                {
                    Reference = new()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
    });

    var app = builder.Build();
    
    // Configure the HTTP request pipeline.
    
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseOpenTelemetryPrometheusScrapingEndpoint();
    
    app.UseRequestResponseLogging();

    var commonConfig = builder.Configuration.GetSection(new CommonSettings().SectionName).Get<CommonSettings>();
    if (commonConfig.ShowSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "AppSample api"); });
    }

    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        AllowStatusCode404Response = true,
        ExceptionHandler = RequestExceptionHandler.Handle
    });
    
    /*
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    */

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // Add Static Files Middleware  
    app.UseStaticFiles();

    app.MapControllers();
    app.MapDefaultControllerRoute();

    app.Run();

}
catch (Exception exp)
{
    // NLog: catch setup errors
    logger.Error(exp, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}