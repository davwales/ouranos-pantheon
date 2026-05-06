using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;

public sealed record OptimizeStrategyMessage(
    Id<Backtest> BacktestId,
    uint Generations,
    uint PopulationSize,
    double SharpeRatioWeight = 0.5,
    double TotalReturnWeight = 0.3,
    double MaxDrawdownWeight = -0.2,
    decimal VolumeParticipationRate = 0.25m,
    decimal SlippageMultiplier = 0.1m
)
{
    public const string Exchange = "plutus.backtest";
    public const string Queue = "plutus.backtest.optimize";
    public const string DeadLetterQueue = "plutus.backtest.optimize.dlq";
}
