using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;

public sealed record GetMarketInput(Id<Market> MarketId);
