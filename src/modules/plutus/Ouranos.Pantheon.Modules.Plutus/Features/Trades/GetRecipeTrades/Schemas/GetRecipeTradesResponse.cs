using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;

public sealed record GetRecipeTradesResponse(
    Id<Recipe> RecipeId,
    string RecipeName,
    decimal LatestBuyPrice,
    decimal LatestSellPrice,
    decimal LatestMargin,
    decimal AverageBuyPrice,
    decimal AverageSellPrice,
    decimal AverageMargin
);
