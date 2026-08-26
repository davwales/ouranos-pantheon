using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;

public sealed record GetSignalRankingsResponse(
    Id<Symbol> SymbolId,
    string SymbolName,
    string? SymbolSubcode,
    decimal? DailyAveragePrice,
    decimal? DailyVolume,
    decimal OverallScore,
    decimal? BuyScore,
    decimal? SellScore,
    decimal? FlipScore,
    decimal? MerchScore,
    int SignalCount,
    int BullishCount,
    int BearishCount
);
