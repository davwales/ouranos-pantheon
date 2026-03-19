using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe.Schemas;

public sealed record CreateRecipeInput(
    Id<Market> MarketId,
    string Name,
    decimal Cost,
    ICollection<RecipeComponent> Inputs,
    ICollection<RecipeComponent> Outputs
);
