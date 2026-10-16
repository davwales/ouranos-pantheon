namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

internal sealed record MarketTradesQuery(
    IReadOnlyList<ExplorerCondition> Conditions,
    string[] Filter,
    string? SortField,
    IReadOnlyList<string> Problems
);
