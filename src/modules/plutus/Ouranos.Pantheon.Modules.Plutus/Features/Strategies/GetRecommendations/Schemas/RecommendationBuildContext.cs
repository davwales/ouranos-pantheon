using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;

internal sealed record RecommendationBuildContext(
    List<MarketTradeSnapshot> Snapshots,
    List<Signal> Signals,
    Dictionary<Id<Symbol>, Dictionary<SignalType, IReadOnlyList<decimal>>> SignalHistory,
    Strategy Strategy,
    Id<Market> MarketId,
    decimal TaxRate,
    decimal Limit,
    decimal BuyThreshold,
    decimal Budget,
    decimal MaxPositionPercent,
    IStrategyExecutor Executor
);
