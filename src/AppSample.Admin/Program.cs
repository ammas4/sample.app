using AppSample.Admin;
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

    // Add services to the container.

    ServicesConfig.Configure(builder.Services, builder.Configuration, false);
    builder.Services.AddControllersWithViews();


    //builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

    // builder.Services.AddAuthorization(options =>
    // {
    //     // By default, all incoming requests will be authorized according to the default policy.
    //     options.FallbackPolicy = options.DefaultPolicy;
    // });

    // EF Identity
    //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    //builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //    options.UseSqlServer(connectionString));
    //builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    //    .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddRazorPages();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }

    app.UseStaticFiles();

    app.UseRouting();

    //app.UseAuthentication();
    //app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    //app.MapRazorPages();

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