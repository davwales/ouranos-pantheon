using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Modules.Shared.API.Queries;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades;

[ExtendObjectType<Query>]
public sealed class GetSymbolTradesQuery
{
    /// <summary>
    ///     Retrieves information about the trades for a given symbol.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Query used to filter symbol trades.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Trade statistics for a symbol.</returns>
    public async Task<GetSymbolTradesResponse> GetSymbolTrades(
        [Service] IScopedDispatcher dispatcher,
        GetSymbolTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }
}
