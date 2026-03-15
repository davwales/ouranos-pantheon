using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades;

[ExtendObjectType<Query>]
public sealed class GetRecipeTradesQuery
{
    /// <summary>
    ///     Gets recipe trade information.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Input required to filter recipe trades.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>List of recipe trade information.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<GetRecipeTradesResponse>> GetRecipeTrades(
        [Service] IScopedDispatcher dispatcher,
        GetRecipeTradesInput input,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(input, cancellationToken);
        return wrapper.Value;
    }
}
