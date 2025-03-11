using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.API.Queries;

[ExtendObjectType<Query>]
public sealed class TradeQueries
{
    /// <summary>
    ///     Gets all trades.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>List of all trades.</returns>
    [UsePaging(IncludeTotalCount = false)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Trade>> GetAllTrades(
        [Service] IDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllEntitiesInput<Trade>(), cancellationToken);
        return wrapper.Value;
    }
}