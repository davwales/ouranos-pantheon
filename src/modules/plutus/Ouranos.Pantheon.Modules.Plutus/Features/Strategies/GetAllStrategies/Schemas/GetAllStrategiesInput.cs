using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies.Schemas;

public sealed record GetAllStrategiesInput(
    Id<Market> MarketId,
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10,
    string[]? Filter = null
);