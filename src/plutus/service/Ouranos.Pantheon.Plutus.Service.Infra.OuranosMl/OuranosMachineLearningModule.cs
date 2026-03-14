using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Infra.OuranosMl.Forecasts;

namespace Ouranos.Pantheon.Plutus.Service.Infra.OuranosMl;

public static class OuranosMachineLearningModule
{
    public static IServiceCollection AddOuranosMachineLearningModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddCoreOuranosMachineLearningModule(configuration)
            .AddScoped<IGetForecastPredictions, GetForecastPredictions>();
    }
}