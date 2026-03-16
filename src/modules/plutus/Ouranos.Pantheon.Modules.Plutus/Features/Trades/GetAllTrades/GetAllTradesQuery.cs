using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Ouranos.Pantheon.Modules.Shared.API.Queries;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades;

[ExtendObjectType<Query>]
public sealed class GetAllTradesQuery
{
    /// <summary>
    ///     Gets a queryable list of trades.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />.
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>A list of trades.</returns>
    [UsePaging(IncludeTotalCount = false)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Trade>> GetAllTrades(
        [Service] IScopedDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllTradesInput(), cancellationToken);
        return wrapper.Value;
    }
}
