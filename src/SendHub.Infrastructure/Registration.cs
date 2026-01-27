using Microsoft.Extensions.DependencyInjection;
using SendHub.Infrastructure.FileSystem;

namespace SendHub.Infrastructure;

public static class Registration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSendHubInfrastructure()
        {
            return services
                .AddSingleton<IFileScanner, FileSystemScanner>()
                .AddSingleton<IFileSystemWatcher, FileSystemWatcherAdapter>()
                .AddSingleton<IFileSystemWatcherFactory, FileSystemWatcherFactory>();
        }
    }
}
