using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;

public sealed record GetRecipeInput(Id<Recipe> RecipeId, TimeFrame TimeFrame = TimeFrame.OneHour);
