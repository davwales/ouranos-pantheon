namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels.Schemas;

public sealed record GetAllModelsInput(
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
