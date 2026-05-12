using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;

public sealed record RunBacktestMessage(
    Id<Backtest> BacktestId,
    decimal VolumeParticipationRate = 0.25m,
    decimal SlippageMultiplier = 0.1m
)
{
    public const string Exchange = "plutus.backtest";
    public const string Queue = "plutus.backtest.run";
    public const string DeadLetterQueue = "plutus.backtest.run.dlq";
}
