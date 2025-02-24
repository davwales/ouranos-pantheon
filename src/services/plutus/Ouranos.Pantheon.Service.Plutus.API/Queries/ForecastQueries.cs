using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

namespace Ouranos.Pantheon.Service.Plutus.API.Queries;

[ExtendObjectType<Query>]
public sealed class ForecastQueries
{
    /// <summary>
    ///     Gets all forecasts.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>List of all forecasts.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Forecast>> GetAllForecasts(
        [Service] IDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllEntitiesInput<Forecast>(), cancellationToken);
        return wrapper.Value;
    }
}