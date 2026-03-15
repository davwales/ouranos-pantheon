using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;

[ExtendObjectType<Mutation>]
public sealed class DeleteMarketMutation
{
    /// <summary>
    ///     Deletes a market.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="marketId">Id of the market to delete.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the recently deleted market.</returns>
    public async Task<IdResponse<Market>> DeleteMarket(
        [Service] IScopedDispatcher dispatcher,
        Id<Market> marketId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new DeleteMarketInput(marketId), cancellationToken);
    }
}
