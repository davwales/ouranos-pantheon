using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;

public interface ISymbolScope : IMarketScope
{
    Id<Symbol> SymbolId { get; }
}
