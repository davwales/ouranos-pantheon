using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

/// <summary>
///     Canonical enumeration of every weighted input a strategy can blend. Values
///     correspond one-to-one with the seven <see cref="Signals.SignalType" /> signal
///     computers; each kind is scored by its dedicated <see cref="SignalInputScorerBase" />
///     subclass and blended by the executor using the strategy's weight vector.
/// </summary>
public enum InputKind
{
    SignalTaxAdjustedRoi = 1,
    SignalVolumeAnomaly = 2,
    SignalTrendMomentum = 3,
    SignalBollingerBands = 4,
    SignalRsi = 5,
    SignalMovingAverageCrossover = 6,
    SignalPriceVelocity = 7,
}
