using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Recipes;

public sealed record RecipeComponent(
    Id<Symbol> SymbolId,
    string Name,
    int Quantity
)
{
    private RecipeComponent() : this(new Id<Symbol>(Guid.NewGuid().ToString()), string.Empty, 0)
    {
    }
}