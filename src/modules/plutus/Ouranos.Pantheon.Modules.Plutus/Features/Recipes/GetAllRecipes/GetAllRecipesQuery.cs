using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Ouranos.Pantheon.Modules.Shared.API.Queries;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes;

[ExtendObjectType<Query>]
public sealed class GetAllRecipesQuery
{
    /// <summary>
    ///     Gets a queryable list of recipes.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />.
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>A list of recipes.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Recipe>> GetAllRecipes(
        [Service] IScopedDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllRecipesInput(), cancellationToken);
        return wrapper.Value;
    }
}
