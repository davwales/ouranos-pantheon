using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.SetExplorerView.Schemas;

public sealed record SetExplorerViewAction(
    IReadOnlyList<ExplorerCondition> Conditions,
    string? SortField,
    bool Descending
);
