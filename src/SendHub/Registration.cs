using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendHub.Features.Email.SendFile;
using SendHub.Features.Folder.MoveFile;
using SendHub.Features.Folder.Scan;
using SendHub.Features.Teams.SendFile;

namespace SendHub;

public static class Registration
{
    public static IServiceCollection AddSendHub(
        this IServiceCollection services)
    {
        return services
            .AddTransient<ICommandHandler<ScanFolder, ScanFolderResult>, ScanFolderHandler>()
            .AddTransient<ICommandHandler<MoveFile>, MoveFileHandler>();
    }

    public static IServiceCollection AddEmail(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<EmailSenderConfig>()
            .Bind(configuration.GetSection("Email"));

        return services
            .AddTransient<IFileSender, EmailSender>();
    }

    public static IServiceCollection AddTeams(
        this IServiceCollection services)
    {
        return services
            .AddTransient<IFileSender, TeamsSender>();
    }
}
