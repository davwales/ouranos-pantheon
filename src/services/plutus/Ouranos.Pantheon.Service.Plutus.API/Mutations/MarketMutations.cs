using MediatR;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.CreateMarket;
using Ouranos.Pantheon.Service.Plutus.Application.Commands.Markets.UpdateMarket;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.API.Mutations;

[ExtendObjectType<Mutation>]
public sealed class MarketMutations
{
    /// <summary>
    ///     Creates a market.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="input">Requested creation data.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>Response containing the created market's identifier.</returns>
    public async Task<IdResponse<Market>> CreateMarket(
        [Service] IMediator mediator,
        CreateMarketInput input,
        CancellationToken cancellationToken = default
    ) => await mediator.Send(input, cancellationToken);

    /// <summary>
    ///     Updates a market by it's identifier.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="input">Requested updated data.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>Status code.</returns>
    public async Task<IdResponse<Market>> UpdateMarket(
        [Service] IMediator mediator,
        UpdateMarketInput input,
        CancellationToken cancellationToken = default
    ) => await mediator.Send(input, cancellationToken);

    /// <summary>
    ///     Deletes a market by it's identifier.
    /// </summary>
    /// <param name="mediator">
    ///     <see cref="IMediator" />
    /// </param>
    /// <param name="marketId">Id of the market to delete.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>Status code.</returns>
    public async Task<IdResponse<Market>> DeleteMarket(
        [Service] IMediator mediator,
        Id<Market> marketId,
        CancellationToken cancellationToken = default
    ) => await mediator.Send(new DeleteEntityInput<Market>(marketId), cancellationToken);
}