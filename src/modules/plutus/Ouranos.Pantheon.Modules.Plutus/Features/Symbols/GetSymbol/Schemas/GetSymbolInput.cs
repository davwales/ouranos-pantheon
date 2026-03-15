using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;

public sealed record GetSymbolInput(
    Id<Symbol> SymbolId
) : IQuery<Symbol>;
