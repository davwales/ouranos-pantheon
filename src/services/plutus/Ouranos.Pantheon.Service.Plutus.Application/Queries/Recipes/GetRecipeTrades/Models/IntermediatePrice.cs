namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Recipes.GetRecipeTrades.Models;

public record IntermediatePrice(
    decimal AveragePrice,
    decimal LatestPrice
);