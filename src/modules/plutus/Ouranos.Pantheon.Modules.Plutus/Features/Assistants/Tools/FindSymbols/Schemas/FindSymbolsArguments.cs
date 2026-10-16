using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.FindSymbols.Schemas;

public sealed record FindSymbolsArguments(
    IReadOnlyList<MarketTradesCondition> Conditions,
    MarketTradesField? SortBy,
    bool Descending,
    int? Take
);
