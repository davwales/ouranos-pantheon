using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;

public sealed record GetMarketForecastInput(
    Id<Market> MarketId
) : IQuery<WrapperResponse<IQueryable<GetMarketForecastResponse>>>;
