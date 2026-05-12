using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;

public sealed record PricedRecipeComponent(
    Id<Symbol> SymbolId,
    string Name,
    int Quantity,
    decimal? LatestPrice,
    decimal? AveragePrice,
    decimal? TotalValue,
    decimal? Volume
);
