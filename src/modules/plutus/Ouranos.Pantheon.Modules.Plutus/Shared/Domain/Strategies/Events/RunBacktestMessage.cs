using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;

public sealed record RunBacktestMessage(Id<Backtest> BacktestId)
{
    public const string Exchange = "plutus.backtest";
    public const string Queue = "plutus.backtest.run";
    public const string DeadLetterQueue = "plutus.backtest.run.dlq";
}