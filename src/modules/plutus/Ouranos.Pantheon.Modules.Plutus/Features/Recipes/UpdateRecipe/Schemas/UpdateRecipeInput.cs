using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe.Schemas;

public sealed record UpdateRecipeInput(
    Id<Market> MarketId,
    Id<Recipe> RecipeId,
    string Name,
    decimal Cost,
    ICollection<RecipeComponent> Inputs,
    ICollection<RecipeComponent> Outputs
) : ICommand<IdResponse<Recipe>>;
