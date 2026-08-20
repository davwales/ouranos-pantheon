using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;

public sealed class PriceVelocityInputScorer : SignalInputScorerBase
{
    public override InputKind Kind => InputKind.SignalPriceVelocity;

    public override SignalType SignalType => SignalType.PriceVelocity;
}
