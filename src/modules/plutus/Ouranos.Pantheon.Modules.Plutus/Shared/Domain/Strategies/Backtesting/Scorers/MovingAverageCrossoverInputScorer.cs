using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;

public sealed class MovingAverageCrossoverInputScorer : SignalInputScorerBase
{
    public override InputKind Kind => InputKind.SignalMovingAverageCrossover;

    public override SignalType SignalType => SignalType.MovingAverageCrossover;
}
