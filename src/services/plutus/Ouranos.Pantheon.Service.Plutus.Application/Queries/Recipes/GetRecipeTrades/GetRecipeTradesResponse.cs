using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Recipes.GetRecipeTrades;

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