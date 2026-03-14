namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record AssistantInput(
    string Model,
    string SystemPrompt,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null
);
