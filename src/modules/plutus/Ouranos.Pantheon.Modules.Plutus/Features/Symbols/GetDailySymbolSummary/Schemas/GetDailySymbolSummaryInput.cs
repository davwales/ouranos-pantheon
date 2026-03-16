using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;

public sealed record GetDailySymbolSummaryInput(
    Id<Symbol> SymbolId
) : IQuery<GetDailySymbolSummaryResponse>;
