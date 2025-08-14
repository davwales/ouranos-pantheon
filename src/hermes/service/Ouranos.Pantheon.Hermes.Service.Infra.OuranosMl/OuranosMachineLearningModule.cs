using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Hermes.Service.Application.Interfaces.Conversations;
using Ouranos.Pantheon.Hermes.Service.Infra.OuranosMl.Conversations;

namespace Ouranos.Pantheon.Hermes.Service.Infra.OuranosMl;

public static class OuranosMachineLearningModule
{
    public static IServiceCollection AddOuranosMachineLearningModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddCoreOuranosMachineLearningModule(configuration)
            .AddScoped<IGenerateChatCompletion, GenerateChatCompletion>();
    }
}