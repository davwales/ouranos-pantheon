using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Recipes.CreateRecipe;

public sealed record CreateRecipeInput(
    Id<Market> MarketId,
    string Name,
    decimal Cost,
    ICollection<RecipeComponent> Inputs,
    ICollection<RecipeComponent> Outputs
) : ICommand<IdResponse<Recipe>>;