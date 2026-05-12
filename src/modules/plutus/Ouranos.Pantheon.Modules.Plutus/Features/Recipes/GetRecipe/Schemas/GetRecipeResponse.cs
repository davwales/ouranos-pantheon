using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;

public sealed record GetRecipeResponse(
    Id<Recipe> Id,
    Id<Market> MarketId,
    string Name,
    decimal Cost,
    IReadOnlyList<PricedRecipeComponent> Inputs,
    IReadOnlyList<PricedRecipeComponent> Outputs
);
