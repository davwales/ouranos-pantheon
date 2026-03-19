using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;

public sealed record DeleteMarketInput(
    Id<Market> MarketId
);
