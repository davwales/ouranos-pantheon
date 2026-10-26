using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests.Schemas;

public sealed record GetActiveBacktestsInput(
    Id<Market>? MarketId = null,
    int Skip = 0,
    int Take = 10
);
