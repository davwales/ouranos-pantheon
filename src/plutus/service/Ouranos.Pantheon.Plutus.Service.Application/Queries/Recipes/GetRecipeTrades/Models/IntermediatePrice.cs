namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Recipes.GetRecipeTrades.Models;

public record IntermediatePrice(
    decimal AveragePrice,
    decimal LatestPrice
);