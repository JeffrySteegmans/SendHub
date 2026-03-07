using System.IO.Abstractions;
using MudBlazor.Services;
using NodaTime;
using SendHub.Features;
using SendHub.Infrastructure;
using SendHub.Web;
using SendHub.Web.Components;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    #if DEBUG
    .MinimumLevel.Verbose()
    #endif
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ApplicationName", "SendHub.Web")
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithUtcTime()
    .WriteTo.Console(theme: SystemConsoleTheme.None)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Logging
    .ClearProviders()
    .AddSerilog(Log.Logger, dispose: true);

builder.AddServiceDefaults();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services
    .AddSendHubFeatures()
    .AddSendHubInfrastructure()
    .AddHostedService<FolderWatcher>()
    .AddTransient<IFileSystem, FileSystem>()
    .AddSingleton<IClock>(SystemClock.Instance);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

await app.RunAsync();
