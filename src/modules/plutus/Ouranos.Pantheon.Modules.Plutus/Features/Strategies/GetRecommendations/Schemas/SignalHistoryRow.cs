namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;

internal sealed record SignalHistoryRow(
    Guid SymbolId,
    int SignalType,
    DateTimeOffset Bucket,
    decimal? LastValue
);
