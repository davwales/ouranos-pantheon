namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras.Schemas;

public sealed record GetAllLorasInput(
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
