using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe.Schemas;

public sealed record DeleteRecipeInput(Id<Recipe> RecipeId);
