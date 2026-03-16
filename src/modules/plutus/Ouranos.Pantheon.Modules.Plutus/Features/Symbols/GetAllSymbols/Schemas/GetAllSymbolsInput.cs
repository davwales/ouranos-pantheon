using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;

public sealed record GetAllSymbolsInput : IQuery<WrapperResponse<IQueryable<Symbol>>>;
