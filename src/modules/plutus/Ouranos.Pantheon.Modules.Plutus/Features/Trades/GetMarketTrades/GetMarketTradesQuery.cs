using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;

[ExtendObjectType<Query>]
public sealed class GetMarketTradesQuery
{
    /// <summary>
    ///     Retrieves information about the trades for a given market.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Query used to filter market trades.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Trade statistics for the symbols in a market.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<GetMarketTradesResponse>> GetMarketTrades(
        [Service] IScopedDispatcher dispatcher,
        GetMarketTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(input, cancellationToken);
        return wrapper.Value;
    }
}
