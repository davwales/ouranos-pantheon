using MediatR;
using Talos.Olympus.Core.API.Queries;
using Talos.Olympus.Core.Application.Queries.Common.GetAllEntities;
using Talos.Olympus.Core.Application.Queries.Common.GetEntity;
using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Plutus.Domain.Markets;

namespace Talos.Olympus.Service.Plutus.API.Queries;

[ExtendObjectType<Query>]
public sealed class MarketQueries
{
    /// <summary>
    ///     Gets all markets.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>List of all markets.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Market>> GetAllMarkets(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(new GetAllEntitiesInput<Market>(), cancellationToken);
    }

    /// <summary>
    ///     Retrieves a market by it's identifier.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="marketId">Id of the market to get.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>The market matching the given query.</returns>
    public async Task<Market> GetMarket(
        [Service] IMediator mediator,
        Id<Market> marketId,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(new GetEntityInput<Market>(marketId), cancellationToken);
    }
}