using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;

public sealed record GetAllRecipesResponse(
    Id<Recipe> Id,
    Id<Market> MarketId,
    string Name,
    decimal Cost
);
