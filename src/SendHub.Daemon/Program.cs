using SendHub;
using SendHub.Daemon;
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
    .AddEnvironmentVariables(prefix: "SendHub_");

builder.Logging
    .ClearProviders()
    .AddSerilog(Log.Logger, dispose: true);

builder.Services
    .AddSendHub()
    .AddEmail(builder.Configuration)
    .AddTeams()
    .AddHostedService<FolderWatcher>();

using var host = builder
    .Build();

await host
    .RunAsync();
