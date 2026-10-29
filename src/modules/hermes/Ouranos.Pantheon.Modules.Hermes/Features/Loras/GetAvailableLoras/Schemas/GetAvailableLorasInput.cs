namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras.Schemas;

public sealed record GetAvailableLorasInput(
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
