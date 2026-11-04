namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;

public sealed record SignalSummary(
    decimal AggregatedScore,
    int BullishCount,
    int BearishCount,
    int NeutralCount,
    bool IsFlipFavourable,
    bool IsMerchFavourable
);
