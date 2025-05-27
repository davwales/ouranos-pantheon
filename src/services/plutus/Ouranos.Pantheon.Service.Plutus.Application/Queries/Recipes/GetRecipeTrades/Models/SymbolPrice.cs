using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Recipes.GetRecipeTrades.Models;

public record SymbolPrice(
    Id<Symbol> Id,
    decimal AveragePrice,
    decimal LatestPrice
);