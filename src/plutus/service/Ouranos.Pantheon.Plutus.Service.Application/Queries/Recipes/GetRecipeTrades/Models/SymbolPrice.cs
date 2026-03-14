using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Recipes.GetRecipeTrades.Models;

public record SymbolPrice(
    Id<Symbol> Id,
    decimal AveragePrice,
    decimal LatestPrice
);