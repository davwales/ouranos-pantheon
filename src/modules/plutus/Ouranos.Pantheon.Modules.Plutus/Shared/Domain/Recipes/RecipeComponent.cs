using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

public record RecipeComponent(
    Id<Symbol> SymbolId,
    string Name,
    int Quantity
)
{
    protected RecipeComponent() : this(
        new Id<Symbol>(Guid.NewGuid().ToString()),
        string.Empty,
        0
    )
    {
    }
}
