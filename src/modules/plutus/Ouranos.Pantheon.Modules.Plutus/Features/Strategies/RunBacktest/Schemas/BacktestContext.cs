using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed record BacktestContext(
    BacktestData Data,
    IStrategyExecutor Executor,
    decimal TaxRate,
    DateTimeOffset StartDate,
    IReadOnlyList<InputWeight> InputWeights,
    InputThresholds Thresholds
)
{
    public DateTimeOffset CurrentDate(PipelineContext ctx)
    {
        return StartDate.AddDays(ctx.CurrentIteration);
    }
}
