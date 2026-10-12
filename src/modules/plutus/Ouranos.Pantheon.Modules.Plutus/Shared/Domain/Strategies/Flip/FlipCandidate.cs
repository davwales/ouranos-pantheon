namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <param name="Quote">Bid and ask for the symbol.</param>
/// <param name="ExpectedUnits">
///     Units a buy and a sell order at these prices would each have filled per check-in over the
///     lookback, capped by the buy limit.
/// </param>
/// <param name="ExpectedProfit">Net margin x bid x expected units.</param>
public sealed record FlipCandidate(FlipQuote Quote, decimal ExpectedUnits, decimal ExpectedProfit);
