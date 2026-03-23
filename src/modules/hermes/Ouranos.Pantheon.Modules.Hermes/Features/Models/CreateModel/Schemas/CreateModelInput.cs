namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel.Schemas;

public sealed record CreateModelInput(
    string Name,
    string ModelIdentifier,
    string SystemPrompt,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null,
    bool IsDefault = false
);
