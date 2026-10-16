using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSignalRankings.Schemas;

public sealed record SignalRankingData(
    Id<Symbol> SymbolId,
    string Name,
    decimal? Score,
    int BullishCount,
    int BearishCount,
    decimal? DailyAveragePrice,
    decimal? DailyVolume
);
