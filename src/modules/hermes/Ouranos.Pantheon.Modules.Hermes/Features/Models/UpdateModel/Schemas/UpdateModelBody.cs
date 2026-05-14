namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel.Schemas;

public sealed record UpdateModelBody(
    string Name,
    string ModelIdentifier,
    string SystemPrompt,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null,
    int? ContextWindow = null,
    bool IsDefault = false,
    bool IsPublic = true
);
