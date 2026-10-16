using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskSymbolAnalyst.Schemas;

internal sealed record SymbolAnalystSnapshot(
    GetSymbolResponse Symbol,
    GetMarketResponse Market,
    TimeFrame TimeFrame,
    GetDailySymbolSummaryResponse Today,
    DateTimeOffset GeneratedAt
);
