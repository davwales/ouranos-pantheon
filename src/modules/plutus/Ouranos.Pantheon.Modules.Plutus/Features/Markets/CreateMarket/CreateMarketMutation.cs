using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Modules.Shared.API.Mutations;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket;

[ExtendObjectType<Mutation>]
public sealed class CreateMarketMutation
{
    /// <summary>
    ///     Creates a market.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Command to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the newly created market.</returns>
    public async Task<IdResponse<Market>> CreateMarket(
        [Service] IScopedDispatcher dispatcher,
        CreateMarketInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }
}
