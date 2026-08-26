using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;

/// <summary>
///     Aggregated result of <c>GetRecommendationsHandler.LoadRecommendationDataAsync</c>:
///     everything the recommendation loop needs, loaded once up front so the handler
///     body stays a plain validate -&gt; load -&gt; loop -&gt; sort flow.
/// </summary>
internal sealed record RecommendationData(
    Strategy Strategy,
    Market Market,
    List<Symbol> Symbols,
    List<MarketTradeSnapshot> Snapshots,
    List<Signal> Signals,
    Dictionary<Id<Symbol>, Dictionary<SignalType, IReadOnlyList<decimal>>> SignalHistory
);
