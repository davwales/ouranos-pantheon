using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;

public sealed record GetSignalRankingsInput(
    Id<Market> MarketId,
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10,
    string[]? Filter = null
);
