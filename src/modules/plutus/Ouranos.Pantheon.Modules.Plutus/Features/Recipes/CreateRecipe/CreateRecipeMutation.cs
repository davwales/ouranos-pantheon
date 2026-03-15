using HotChocolate;
using HotChocolate.Types;
using Ouranos.Pantheon.Core.API.Mutations;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe;

[ExtendObjectType<Mutation>]
public sealed class CreateRecipeMutation
{
    /// <summary>
    ///     Creates a recipe.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="input">Command to be executed.</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>The id of the newly created recipe.</returns>
    public async Task<IdResponse<Recipe>> CreateRecipe(
        [Service] IScopedDispatcher dispatcher,
        CreateRecipeInput input,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(input, cancellationToken);
    }
}
