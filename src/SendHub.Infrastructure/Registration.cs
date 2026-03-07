using Microsoft.Extensions.DependencyInjection;
using SendHub.Infrastructure.Database;
using SendHub.Infrastructure.FileSystem;
using SendHub.Infrastructure.Messaging.Email;
using SendHub.Infrastructure.Settings;
using SendHub.Infrastructure.Tracking;

namespace SendHub.Infrastructure;

public static class Registration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSendHubInfrastructure()
        {
            services
                .AddOptions<DatabaseSettings>()
                .BindConfiguration(DatabaseSettings.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services
                .AddSingleton<IApplicationSettings, SqliteApplicationSettings>()
                .AddSingleton<IFileScanner, FileSystemScanner>()
                .AddSingleton<IFileSystemWatcher, FileSystemWatcherAdapter>()
                .AddSingleton<IFileSystemWatcherFactory, FileSystemWatcherFactory>()
                .AddSingleton<IFileSender, SmtpFileSender>()
                .AddSingleton<IFileArchiver, FileArchiver>()
                .AddSingleton<IProcessedFileTracker, SqliteFileTracker>();
        }
    }
}
