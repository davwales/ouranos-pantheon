namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <param name="Series">The quoted symbol.</param>
/// <param name="Bid">Buy order price, in whole currency units.</param>
/// <param name="Ask">Sell order price, in whole currency units.</param>
/// <param name="LowPrice">Recent sell-side VWAP; where unsold inventory is dumped.</param>
/// <param name="NetMargin">Profit per unit after tax, as a fraction of the bid.</param>
public sealed record FlipQuote(
    FlipSeries Series,
    decimal Bid,
    decimal Ask,
    decimal LowPrice,
    decimal NetMargin
);
