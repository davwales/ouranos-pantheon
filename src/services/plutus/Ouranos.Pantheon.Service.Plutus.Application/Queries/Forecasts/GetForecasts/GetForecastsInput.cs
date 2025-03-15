using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetForecasts;

public sealed record GetForecastsInput : IQuery<WrapperResponse<IQueryable<GetForecastsResponse>>>;