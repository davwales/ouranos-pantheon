using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Infra.OuranosMl.Forecasts;

namespace Ouranos.Pantheon.Service.Plutus.Infra.OuranosMl;

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