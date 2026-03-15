using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;

public sealed record GetRecipeInput(
    Id<Recipe> RecipeId
) : IQuery<Recipe>;
