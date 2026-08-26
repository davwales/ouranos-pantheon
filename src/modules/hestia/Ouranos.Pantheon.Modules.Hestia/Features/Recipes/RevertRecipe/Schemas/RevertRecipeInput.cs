using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe.Schemas;

public sealed record RevertRecipeInput(Id<Recipe> RecipeId, long TargetVersion);
