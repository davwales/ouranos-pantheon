using MediatR;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Trades.GetMarketTrades;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Trades.GetSymbolTrades;

namespace Ouranos.Pantheon.Service.Plutus.API.Queries;

[ExtendObjectType<Query>]
public sealed class TradeQueries
{
    /// <summary>
    ///     Retrieves information about the symbols in a market.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="input">Query used to filter market trades.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>Trade statistics for the symbols in a market.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<GetMarketTradesResponse>> GetMarketTrades(
        [Service] IMediator mediator,
        GetMarketTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(input, cancellationToken);
    }

    /// <summary>
    ///     Retrieves information about the trades for a given symbol.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="input">Query used to filter symbol trades.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Trade statistics for a symbol.</returns>
    public async Task<GetSymbolTradesResponse> GetSymbolTrades(
        [Service] IMediator mediator,
        GetSymbolTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(input, cancellationToken);
    }
}