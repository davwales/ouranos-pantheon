using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Sizing;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;

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
    string Rationale
);
