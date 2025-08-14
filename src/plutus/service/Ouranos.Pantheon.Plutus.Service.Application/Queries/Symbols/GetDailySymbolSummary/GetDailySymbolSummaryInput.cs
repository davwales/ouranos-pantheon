using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Symbols.GetDailySymbolSummary;

public sealed record GetDailySymbolSummaryInput(
    Id<Symbol> SymbolId
) : IQuery<GetDailySymbolSummaryResponse>;