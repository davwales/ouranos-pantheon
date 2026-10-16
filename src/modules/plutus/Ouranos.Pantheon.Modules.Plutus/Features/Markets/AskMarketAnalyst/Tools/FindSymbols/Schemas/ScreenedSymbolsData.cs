namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.FindSymbols.Schemas;

public sealed record ScreenedSymbolsData(
    int TotalMatches,
    IReadOnlyList<ScreenedSymbolData> Symbols,
    IReadOnlyList<string>? IgnoredConditions
);
