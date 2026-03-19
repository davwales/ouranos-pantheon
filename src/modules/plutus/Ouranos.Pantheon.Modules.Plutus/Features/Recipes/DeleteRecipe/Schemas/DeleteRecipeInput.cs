using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe.Schemas;

public sealed record DeleteRecipeInput(
    Id<Recipe> RecipeId
);
