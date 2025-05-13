using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Application.Commands.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;

namespace Ouranos.Pantheon.Service.Plutus.API.Mutations;

[ExtendObjectType<Mutation>]
public sealed class RecipeMutations
{
    /// <summary>
    ///     Updates a recipe.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Requested updated data.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>The id of the updated recipe.</returns>
    public async Task<IdResponse<Recipe>> UpdateRecipe(
        [Service] IDispatcher dispatcher,
        UpdateRecipeInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }
}