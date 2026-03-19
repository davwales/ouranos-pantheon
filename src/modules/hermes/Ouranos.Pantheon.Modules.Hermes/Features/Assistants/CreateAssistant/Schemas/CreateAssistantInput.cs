namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant.Schemas;

public sealed record CreateAssistantInput(
    string Model,
    string SystemPrompt,
    string? AssistantName = null,
    string? UserName = null,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null
);