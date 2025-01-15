using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;
using Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl;

public static class OuranosMachineLearningModule
{
    public static IServiceCollection AddOuranosMachineLearningModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .Configure<OuranosMachineLearningOptions>(
                configuration.GetSection(OuranosMachineLearningOptions.SectionName))
            .AddCoreOuranosMachineLearningModule(configuration)
            .AddScoped<IGenerateCompletion, GenerateCompletion>();
    }
}