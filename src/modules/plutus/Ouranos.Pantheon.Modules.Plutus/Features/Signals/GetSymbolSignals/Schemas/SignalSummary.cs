namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;

public sealed record SignalSummary(
    decimal AggregatedScore,
    int BullishCount,
    int BearishCount,
    int NeutralCount,
    bool IsFlipFavourable,
    bool IsMerchFavourable
);
