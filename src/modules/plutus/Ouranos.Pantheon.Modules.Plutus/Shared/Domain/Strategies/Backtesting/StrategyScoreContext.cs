using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;

public sealed record StrategyScoreContext(
    Id<Symbol> SymbolId,
    Id<Market> MarketId,
    string SymbolName,
    string? SymbolSubcode,
    decimal CurrentPrice,
    decimal TaxRate,
    decimal Limit,
    MarketTradeSnapshot? ShortSnapshot,
    MarketTradeSnapshot? MediumSnapshot,
    MarketTradeSnapshot? LongSnapshot,
    IReadOnlyList<PriceBucket> PriceBuckets,
    IReadOnlyList<Signal> Signals,
    decimal? ForecastedPrice,
    decimal? ForecastedPriceChange,
    SignalWeightedConfig? SignalWeightedConfig = null,
    ForecastMomentumConfig? ForecastMomentumConfig = null,
    MeanReversionConfig? MeanReversionConfig = null,
    RecipeArbitrageConfig? RecipeArbitrageConfig = null,
    List<CompositeComponent>? Components = null
);
