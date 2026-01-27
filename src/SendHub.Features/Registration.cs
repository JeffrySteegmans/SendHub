using Microsoft.Extensions.DependencyInjection;
using SendHub.Features.FileProcessing;

namespace SendHub.Features;

public static class Registration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSendHubFeatures()
        {
            return services
                .AddTransient<ICommandHandler<ProcessIncomingFile>, ProcessIncomingFileHandler>();
        }
    }
}
