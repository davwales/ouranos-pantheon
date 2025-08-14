using Ouranos.Pantheon.Core.API.Queries;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Queries.Recipes.GetRecipeTrades;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;

namespace Ouranos.Pantheon.Plutus.Service.API.Queries;

[ExtendObjectType<Query>]
public sealed class RecipeQueries
{
    /// <summary>
    ///     Gets all recipes.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>List of all recipes.</returns>
    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Recipe>> GetAllRecipes(
        [Service] IScopedDispatcher dispatcher,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = await dispatcher.Send(new GetAllEntitiesInput<Recipe>(), cancellationToken);
        return wrapper.Value;
    }

    /// <summary>
    ///     Retrieves a recipe by its identifier.
    /// </summary>
    /// <param name="dispatcher">
    ///     <see cref="IDispatcher" />
    /// </param>
    /// <param name="recipeId">Id of the recipe to get.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken" />.</param>
    /// <returns>The recipe matching the given query.</returns>
    public async Task<Recipe> GetRecipe(
        [Service] IScopedDispatcher dispatcher,
        Id<Recipe> recipeId,
        CancellationToken cancellationToken = default
    )
    {
        return await dispatcher.Send(new GetEntityInput<Recipe>(recipeId), cancellationToken);
    }

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