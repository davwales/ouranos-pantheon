using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPriceHistory.Schemas;

public sealed record PriceHistoryData(
    LatestTradeData? LatestTrade,
    PriceSummaryData Summary,
    decimal? ChangePercent,
    IReadOnlyList<PriceBucketData> Chart
);
