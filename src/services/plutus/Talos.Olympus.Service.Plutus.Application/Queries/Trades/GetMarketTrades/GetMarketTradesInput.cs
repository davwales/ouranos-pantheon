using MediatR;
using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Plutus.Domain.Markets;

namespace Talos.Olympus.Service.Plutus.Application.Queries.Trades.GetMarketTrades;

public sealed record GetMarketTradesInput(
    Id<Market> MarketId,
    double? Seconds = null
) : IRequest<IQueryable<GetMarketTradesResponse>>;