namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;

public sealed record ExplorerView(
    string[]? Filter = null,
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10
);
