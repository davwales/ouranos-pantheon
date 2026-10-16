using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetPriceHistory.Schemas;

public sealed record PriceHistoryData(
    LatestTradeData? LatestTrade,
    PriceSummaryData Summary,
    decimal? ChangePercent,
    IReadOnlyList<PriceBucketData> Chart
);
