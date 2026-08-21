using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

namespace Ouranos.Pantheon.Modules.Plutus.Shared;

public sealed record PlutusOptions(
    DataLoadersOptions DataLoaders,
    ForecastingOptions Forecasting,
    OptimizationOptions Optimization
)
{
    public const string SectionName = "Ouranos:Plutus";

    public PlutusOptions()
        : this(
            DataLoaders: new DataLoadersOptions(),
            Forecasting: new ForecastingOptions(),
            Optimization: new OptimizationOptions()
        ) { }
}
