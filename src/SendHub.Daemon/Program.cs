using System.IO.Abstractions;
using NodaTime;
using SendHub.Daemon;
using SendHub.Features;
using SendHub.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    #if DEBUG
    .MinimumLevel.Verbose()
    #endif
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ApplicationName", "SendHub.Daemon")
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithUtcTime()
    .WriteTo.Console(theme: SystemConsoleTheme.None)
    .CreateLogger();

var builder = Host
    .CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services
    .AddOptions<FolderWatcherSettings>()
    .BindConfiguration(FolderWatcherSettings.SectionName)
    .ValidateOnStart();

builder.Logging
    .ClearProviders()
    .AddSerilog(Log.Logger, dispose: true);

builder.Services
    .AddSendHubFeatures()
    .AddSendHubInfrastructure()
    .AddHostedService<FolderWatcher>()
    .AddTransient<IFileSystem, FileSystem>()
    .AddSingleton<IClock>(SystemClock.Instance);

using var host = builder
    .Build();

await host
    .RunAsync();
