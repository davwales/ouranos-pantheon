using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets;

[ExtendObjectType<Query>]
public sealed class GetAllMarketsQuery
{
    /// <summary>
    ///     Gets a queryable list of markets.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />.
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>A list of markets.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Market>> GetAllMarkets(
        [Service] IScopedDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllMarketsInput(), cancellationToken);
        return wrapper.Value;
    }
}
