using Microsoft.Extensions.DependencyInjection;
using SendHub.Infrastructure.FileSystem;
using SendHub.Infrastructure.Messaging.Email;
using SendHub.Infrastructure.Tracking;

namespace SendHub.Infrastructure;

public static class Registration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSendHubInfrastructure()
        {
            services
                .AddOptions<TrackingSettings>()
                .BindConfiguration(TrackingSettings.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddOptions<SmtpSettings>()
                .BindConfiguration(SmtpSettings.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services
                .AddSingleton<IFileScanner, FileSystemScanner>()
                .AddSingleton<IFileSystemWatcher, FileSystemWatcherAdapter>()
                .AddSingleton<IFileSystemWatcherFactory, FileSystemWatcherFactory>()
                .AddSingleton<IFileSender, SmtpFileSender>()
                .AddSingleton<IFileArchiver, FileArchiver>()
                .AddSingleton<IProcessedFileTracker, JsonFileTracker>();
        }
    }
}
