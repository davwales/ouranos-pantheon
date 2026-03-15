using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe;

[ExtendObjectType<Mutation>]
public sealed class DeleteRecipeMutation
{
    /// <summary>
    ///     Deletes a recipe.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="recipeId">Id of the recipe to delete.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>Identifier of the deleted recipe.</returns>
    public async Task<IdResponse<Recipe>> DeleteRecipe(
        [Service] IScopedDispatcher dispatcher,
        Id<Recipe> recipeId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new DeleteRecipeInput(recipeId), cancellationToken);
    }
}
