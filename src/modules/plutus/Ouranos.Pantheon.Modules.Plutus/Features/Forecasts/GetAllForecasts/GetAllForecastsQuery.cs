using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Ouranos.Pantheon.Modules.Shared.API.Queries;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;

[ExtendObjectType<Query>]
public sealed class GetAllForecastsQuery
{
    /// <summary>
    ///     Gets a queryable list of forecasts.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />.
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>A list of forecasts.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Forecast>> GetAllForecasts(
        [Service] IScopedDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllForecastsInput(), cancellationToken);
        return wrapper.Value;
    }
}
