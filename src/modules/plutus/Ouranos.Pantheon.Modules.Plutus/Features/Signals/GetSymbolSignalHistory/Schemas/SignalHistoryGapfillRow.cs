namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;

internal sealed record SignalHistoryGapfillRow(
    int SignalType,
    DateTimeOffset Bucket,
    decimal? Value
);
