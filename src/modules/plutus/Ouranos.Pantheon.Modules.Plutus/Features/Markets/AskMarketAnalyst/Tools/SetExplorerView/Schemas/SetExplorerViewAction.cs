using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.SetExplorerView.Schemas;

public sealed record SetExplorerViewAction(
    IReadOnlyList<ExplorerCondition> Conditions,
    string? SortField,
    bool Descending
);
