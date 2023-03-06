using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using AppSample.CoreTools.Helpers;
using AppSample.Domain;
using AppSample.Service.Services;
using AppSample.Domain.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using Topshelf;
using Host = Microsoft.Extensions.Hosting.Host;
using ILogger = NLog.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace AppSample.Service;

public class Program
{
    static ILogger? _logger;

    public static async Task Main(string[] args)
    {
        LogManager.LoadConfiguration("nlog.config");
        _logger = LogManager.GetCurrentClassLogger();
        _logger.Debug("Program start");

        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.IsNullOrEmpty(environment))
        {
            environment = "Development";
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
#if DEBUG
            .AddJsonFile("appsettings.MVTerekhin.json", true)
#endif
            .AddEnvironmentVariables()
            .Build();

        // setup our DI
        IServiceCollection services = new ServiceCollection();
        ServicesConfig.Configure(services, configuration);
        services.AddSingleton<IHostApplicationLifetime, MockHostApplicationLifeTime>();
        services.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.SetMinimumLevel(LogLevel.Trace);
            configure.AddNLog();
        });

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var hangfireSettings = serviceProvider.GetRequiredService<IOptions<HangfireSettings>>().Value;

        var memoryLogCts = MemoryLogHelper.StartPeriodicLog(TimeSpan.FromMinutes(5));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            AppDomain.CurrentDomain.ProcessExit += ExitAction;
            AssemblyLoadContext.Default.Unloading += UnloadingAction;

            await Host.CreateDefaultBuilder(args)
                .ConfigureServices(builder =>
                {
                    builder.AddHostedService(sp => new JobService(sp, hangfireSettings));
                })
                .RunConsoleAsync();
        }
        else
        {
            var rc = HostFactory.Run(x =>
            {
                x.Service<JobService>(s =>
                {
                    s.ConstructUsing(_ => new JobService(serviceProvider, hangfireSettings));
                    s.WhenStarted(_ => _.Start());
                    s.WhenStopped(_ => _.Stop());
                });
                x.RunAsLocalService();
                x.SetDescription("AppSample.Service");
                x.SetDisplayName("AppSample.Service");
                x.SetServiceName("AppSample.Service");
            });
        }

        memoryLogCts.Cancel();
        _logger.Debug("Program stop");

        // Ensure to flush and stop internal timers/threads before application-exit 
        LogManager.Shutdown();
    }

    static void ExitAction(object? sender, EventArgs e)
    {
        _logger?.Debug("Program stop in ExitAction");
        LogManager.Shutdown();
        Environment.Exit(0);
    }

    static void UnloadingAction(AssemblyLoadContext obj)
    {
        _logger?.Debug("Program stop in UnloadingAction");
        LogManager.Shutdown();
        Environment.Exit(0);
    }

    static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger?.Error(e.ExceptionObject as Exception, "Program unhandled exception");
    }
}