using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed record BacktestContext(
    BacktestData Data,
    IStrategyExecutor Executor,
    decimal TaxRate,
    int WindowDays,
    DateTimeOffset StartDate,
    SignalWeightedConfig? SignalWeightedConfig = null,
    ForecastMomentumConfig? ForecastMomentumConfig = null,
    MeanReversionConfig? MeanReversionConfig = null,
    RecipeArbitrageConfig? RecipeArbitrageConfig = null
)
{
    public DateTimeOffset CurrentDate(PipelineContext ctx)
    {
        return StartDate.AddDays(ctx.CurrentIteration);
    }
}
