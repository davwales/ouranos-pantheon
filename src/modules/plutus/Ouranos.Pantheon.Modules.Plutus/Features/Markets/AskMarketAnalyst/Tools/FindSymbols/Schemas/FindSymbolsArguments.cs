using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.FindSymbols.Schemas;

public sealed record FindSymbolsArguments(
    IReadOnlyList<MarketTradesCondition> Conditions,
    MarketTradesField? SortBy,
    bool Descending,
    int? Take
);
