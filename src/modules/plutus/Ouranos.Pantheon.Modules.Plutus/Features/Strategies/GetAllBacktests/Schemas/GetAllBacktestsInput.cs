using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests.Schemas;

public sealed record GetAllBacktestsInput(
    Id<Strategy> StrategyId,
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10
);
