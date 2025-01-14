using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Service.Hermes.Application;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;
using Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl;

namespace Ouranos.Pantheon.Service.Hermes.API;

public static class HermesModule
{
    public static IServiceCollection AddHermesModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddApplicationModule()
            .AddOuranosMachineLearningModule(configuration);
    }

    public static IRequestExecutorBuilder AddHermesSchema(this IRequestExecutorBuilder builder)
    {
        return builder.BindModelId<Character>();
    }
}