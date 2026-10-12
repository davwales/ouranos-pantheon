using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;

public sealed record OptimizeStrategyMessage(
    Id<Backtest> BacktestId,
    uint Generations,
    uint PopulationSize,
    double DrawdownWeight = 0.5,
    double OutSampleRatio = 0.2,
    decimal VolumeParticipationRate = BacktestDataOptions.DefaultVolumeParticipationRate,
    decimal SlippageMultiplier = BacktestDataOptions.DefaultSlippageMultiplier,
    int MinTradesPerMonth = 10
)
{
    public const string Exchange = "plutus.backtest";
    public const string Queue = "plutus.backtest.optimize";
    public const string DeadLetterQueue = "plutus.backtest.optimize.dlq";
}
