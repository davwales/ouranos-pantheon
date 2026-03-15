using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;

public sealed record GetRecipeTradesResponse(
    Id<Recipe> Id,
    string Name,
    decimal LatestBuyPrice,
    decimal LatestSellPrice,
    decimal LatestProfit,
    decimal AverageBuyPrice,
    decimal AverageSellPrice,
    decimal AverageProfit
);
