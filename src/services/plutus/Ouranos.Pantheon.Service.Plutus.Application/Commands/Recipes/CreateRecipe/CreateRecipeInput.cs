using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Recipes.CreateRecipe;

public sealed record CreateRecipeInput(
    Id<Market> MarketId,
    string Name,
    decimal Cost,
    IReadOnlyList<RecipeComponent> Inputs,
    IReadOnlyList<RecipeComponent> Outputs
) : ICommand<IdResponse<Recipe>>;