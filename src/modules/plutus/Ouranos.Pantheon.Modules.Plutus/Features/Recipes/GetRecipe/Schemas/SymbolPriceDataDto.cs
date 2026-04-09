namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;

internal sealed record SymbolPriceDataDto(
    decimal LatestPrice,
    decimal AveragePrice,
    decimal Volume
);