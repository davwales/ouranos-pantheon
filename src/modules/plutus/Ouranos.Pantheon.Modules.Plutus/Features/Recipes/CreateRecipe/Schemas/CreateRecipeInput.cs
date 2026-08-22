using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe.Schemas;

public sealed record CreateRecipeInput(
    Id<Market> MarketId,
    string Name,
    decimal Cost,
    ICollection<RecipeComponent> Inputs,
    ICollection<RecipeComponent> Outputs
);
