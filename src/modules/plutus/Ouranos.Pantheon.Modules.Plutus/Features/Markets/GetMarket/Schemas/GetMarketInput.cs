using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;

public sealed record GetMarketInput(
    Id<Market> MarketId
) : IQuery<Market>;
