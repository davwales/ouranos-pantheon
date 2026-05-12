using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket.Schemas;

public sealed record UpdateMarketInput(Id<Market> MarketId, string Name, Taxes Taxes);
