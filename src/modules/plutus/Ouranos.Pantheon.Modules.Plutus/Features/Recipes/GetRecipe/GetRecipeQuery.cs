using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Modules.Shared.API.Queries;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe;

[ExtendObjectType<Query>]
public sealed class GetRecipeQuery
{
    /// <summary>
    ///     Gets a recipe.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="recipeId">The query to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The recipe matching the given query.</returns>
    public async Task<Recipe> GetRecipe(
        [Service] IScopedDispatcher dispatcher,
        Id<Recipe> recipeId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new GetRecipeInput(recipeId), cancellationToken);
    }
}
