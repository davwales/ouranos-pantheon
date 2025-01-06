using MediatR;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Trades.GetMarketTrades;

public sealed record GetMarketTradesInput(
    Id<Market> MarketId,
    double? Seconds = null
) : IRequest<IQueryable<GetMarketTradesResponse>>;