using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.SetExplorerView.Schemas;

public sealed record ExplorerViewArguments(
    IReadOnlyList<MarketTradesCondition> Conditions,
    MarketTradesField? SortBy,
    bool Descending
);
