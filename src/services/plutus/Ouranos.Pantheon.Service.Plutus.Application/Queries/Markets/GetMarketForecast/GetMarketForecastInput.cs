using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Markets.GetMarketForecast;

public sealed record GetMarketForecastInput(
    Id<Market> MarketId
) : IQuery<WrapperResponse<IQueryable<GetMarketForecastResponse>>>;