using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;

public sealed record GetSymbolInput(
    Id<Symbol> SymbolId
) : IQuery<Symbol>;
