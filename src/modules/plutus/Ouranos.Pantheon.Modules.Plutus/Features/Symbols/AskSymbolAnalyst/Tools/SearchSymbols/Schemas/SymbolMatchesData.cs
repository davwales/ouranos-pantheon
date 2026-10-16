namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.SearchSymbols.Schemas;

public sealed record SymbolMatchesData(int TotalMatches, IReadOnlyList<SymbolMatchData> Symbols);
