using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe.Schemas;

public sealed record ToggleRecipeInput(Id<Recipe> RecipeId);
