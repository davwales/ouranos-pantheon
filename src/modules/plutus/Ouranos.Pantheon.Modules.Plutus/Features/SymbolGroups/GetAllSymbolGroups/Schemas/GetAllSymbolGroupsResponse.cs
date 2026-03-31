using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups.Schemas;

public sealed record GetAllSymbolGroupsResponse(
    Id<SymbolGroup> Id,
    Id<Market> MarketId,
    string Name,
    string? Description,
    int SymbolCount,
    decimal? TotalVolume,
    decimal? TotalGain,
    decimal? AverageRoi,
    decimal? AverageOverallScore,
    int BullishCount,
    int BearishCount
);
