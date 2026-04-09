using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;

public sealed record GetRecipeInput(
    Id<Recipe> RecipeId,
    TimeFrame TimeFrame = TimeFrame.OneHour
);
