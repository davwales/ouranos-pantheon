namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits.Schemas;

public sealed record GetAllTraitsInput(
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
