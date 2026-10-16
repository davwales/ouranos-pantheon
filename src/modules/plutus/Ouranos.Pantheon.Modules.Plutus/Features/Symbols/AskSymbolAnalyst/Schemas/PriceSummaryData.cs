using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;

public sealed record PriceSummaryData(
    TimeFrame TimeFrame,
    bool HasTrades,
    decimal? AveragePrice = null,
    decimal? LowPrice = null,
    decimal? HighPrice = null,
    decimal? Volume = null,
    int? Trades = null,
    decimal? MarginAfterTax = null,
    decimal? RoiPercent = null
);
