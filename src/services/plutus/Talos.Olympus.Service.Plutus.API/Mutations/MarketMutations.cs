using MediatR;
using Talos.Olympus.Core.API.Mutations;
using Talos.Olympus.Core.Application.Commands.Common.DeleteEntity;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Core.Domain.Common;
using Talos.Olympus.Service.Plutus.Application.Commands.Markets.CreateMarket;
using Talos.Olympus.Service.Plutus.Application.Commands.Markets.UpdateMarket;
using Talos.Olympus.Service.Plutus.Domain.Markets;

namespace Talos.Olympus.Service.Plutus.API.Mutations;

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
    )
    {
        return await mediator.Send(input, cancellationToken);
    }

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
    )
    {
        return await mediator.Send(input, cancellationToken);
    }

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
    )
    {
        return await mediator.Send(new DeleteEntityInput<Market>(marketId), cancellationToken);
    }
}