using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.FindSymbols.Schemas;

public sealed record ScreenedSymbolData(
    Id<Symbol> SymbolId,
    string Name,
    string? Subcode,
    decimal LowPrice,
    decimal HighPrice,
    decimal? AveragePrice,
    decimal Volume,
    int Trades,
    decimal MarginAfterTax,
    decimal? RoiPercent,
    decimal GainAfterTax
);
