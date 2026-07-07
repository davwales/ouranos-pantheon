using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe.Schemas;

public sealed record GetRecipeInput(Id<Recipe> RecipeId);
