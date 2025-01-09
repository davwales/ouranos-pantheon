using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;
using Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl;

public static class OuranosMachineLearningModule
{
    public static IServiceCollection AddOuranosMachineLearningModule(this IServiceCollection services)
    {
        return services.AddScoped<IGenerateCompletion, GenerateCompletion>();
    }
}