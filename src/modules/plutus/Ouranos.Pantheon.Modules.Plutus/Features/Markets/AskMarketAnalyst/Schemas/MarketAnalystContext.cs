using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;

public sealed record MarketAnalystContext(
    Id<Market> MarketId,
    TimeFrame TimeFrame,
    string[]? Filter = null,
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10
);
