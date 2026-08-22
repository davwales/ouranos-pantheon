using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;

public sealed record GetSymbolInput(Id<Symbol> SymbolId);
