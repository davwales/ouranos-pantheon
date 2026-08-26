using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe.Schemas;

public sealed record ReimportRecipeInput(Id<Recipe> RecipeId);
