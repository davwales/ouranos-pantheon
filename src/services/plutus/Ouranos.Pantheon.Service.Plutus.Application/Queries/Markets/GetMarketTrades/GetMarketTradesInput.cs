using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Markets.GetMarketTrades;

public sealed record GetMarketTradesInput(
    Id<Market> MarketId,
    double? Seconds = null
) : IQuery<WrapperResponse<IQueryable<GetMarketTradesResponse>>>;