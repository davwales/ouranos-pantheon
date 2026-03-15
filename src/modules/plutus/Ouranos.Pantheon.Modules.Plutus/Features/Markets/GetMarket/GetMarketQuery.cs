using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket;

[ExtendObjectType<Query>]
public sealed class GetMarketQuery
{
    /// <summary>
    ///     Gets a market.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="marketId">The query to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The market matching the given query.</returns>
    public async Task<Market> GetMarket(
        [Service] IScopedDispatcher dispatcher,
        Id<Market> marketId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new GetMarketInput(marketId), cancellationToken);
    }
}
