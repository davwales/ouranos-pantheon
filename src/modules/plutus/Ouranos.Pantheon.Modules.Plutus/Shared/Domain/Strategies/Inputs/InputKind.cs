namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

/// <summary>
///     Every weighted input a swing strategy can blend. Each kind is one of the seven
///     <see cref="Signals.SignalType" /> signals; see <see cref="Swing.SwingScoring" />.
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
