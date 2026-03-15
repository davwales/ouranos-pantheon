using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;

public sealed record GetAllForecastsInput : IQuery<WrapperResponse<IQueryable<Forecast>>>;
