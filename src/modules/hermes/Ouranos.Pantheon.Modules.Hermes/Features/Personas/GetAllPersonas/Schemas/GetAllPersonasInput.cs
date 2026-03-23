namespace Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas.Schemas;

public sealed record GetAllPersonasInput(
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
