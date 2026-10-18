namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets.Schemas;

public sealed record GetAllToolSetsInput(
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
