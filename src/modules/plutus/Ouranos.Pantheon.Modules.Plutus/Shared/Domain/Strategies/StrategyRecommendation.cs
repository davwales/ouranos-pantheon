namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record StrategyRecommendation(
    string SymbolId,
    string SymbolName,
    string? SymbolSubcode,
    decimal Score,
    decimal SuggestedAllocation,
    decimal CurrentPrice,
    decimal SuggestedVolume,
    decimal TypicalDailyVolume,
    PositionLimit LimitedBy,
    string Rationale,
    decimal? BuyPrice = null,
    decimal? SellPrice = null,
    decimal? NetMargin = null,
    decimal? ExpectedProfit = null
);
