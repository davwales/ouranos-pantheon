using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe.Schemas;

public sealed record DeleteRecipeInput(
    Id<Recipe> RecipeId
) : ICommand<IdResponse<Recipe>>;
