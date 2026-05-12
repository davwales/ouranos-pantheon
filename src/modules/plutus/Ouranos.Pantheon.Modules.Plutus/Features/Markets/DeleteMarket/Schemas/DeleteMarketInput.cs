using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;

public sealed record DeleteMarketInput(Id<Market> MarketId);
