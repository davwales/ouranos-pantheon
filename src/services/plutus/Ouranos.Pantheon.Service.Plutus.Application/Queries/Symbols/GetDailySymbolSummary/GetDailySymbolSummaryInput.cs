using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Symbols.GetDailySymbolSummary;

public sealed record GetDailySymbolSummaryInput(
    Id<Symbol> SymbolId
) : IQuery<GetDailySymbolSummaryResponse>;