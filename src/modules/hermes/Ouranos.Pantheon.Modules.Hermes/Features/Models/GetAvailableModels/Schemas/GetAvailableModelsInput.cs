namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAvailableModels.Schemas;

public sealed record GetAvailableModelsInput(
    string? SortField = null,
    string? SortDirection = null,
    string[]? Filter = null
);
