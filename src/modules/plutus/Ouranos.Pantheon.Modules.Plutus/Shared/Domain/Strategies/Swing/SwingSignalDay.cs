using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <summary>Signals reconstructed for one symbol from trades up to the day before a decision.</summary>
/// <param name="Price">Latest daily average price in the signal window.</param>
/// <param name="IsEligible">
///     Whether the symbol is liquid enough to be bought that day. Ineligible symbols are still
///     scored so held positions can be sold on their signals.
/// </param>
/// <param name="Signals">Every signal that had enough data.</param>
public sealed record SwingSignalDay(
    decimal Price,
    bool IsEligible,
    IReadOnlyDictionary<SignalType, decimal> Signals
);
