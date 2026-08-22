using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;

public sealed record OptimizeStrategyMessage(
    Id<Backtest> BacktestId,
    uint Generations,
    uint PopulationSize,
    double SortinoWeight = 0.4,
    double CagrWeight = 0.3,
    double DrawdownWeight = 0.5,
    double TurnoverWeight = 0.1,
    double L1RegularizationWeight = 0.05,
    double OutSampleRatio = 0.2,
    decimal VolumeParticipationRate = 0.25m,
    decimal SlippageMultiplier = 0.1m,
    int MinTrades = 5
)
{
    public const string Exchange = "plutus.backtest";
    public const string Queue = "plutus.backtest.optimize";
    public const string DeadLetterQueue = "plutus.backtest.optimize.dlq";
}
