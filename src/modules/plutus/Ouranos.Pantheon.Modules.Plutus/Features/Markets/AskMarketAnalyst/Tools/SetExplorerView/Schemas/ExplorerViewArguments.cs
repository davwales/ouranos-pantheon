using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.SetExplorerView.Schemas;

public sealed record ExplorerViewArguments(
    IReadOnlyList<MarketTradesCondition> Conditions,
    MarketTradesField? SortBy,
    bool Descending
);
