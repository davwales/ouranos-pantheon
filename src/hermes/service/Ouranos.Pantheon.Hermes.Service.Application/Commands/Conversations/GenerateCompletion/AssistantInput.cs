namespace Ouranos.Pantheon.Hermes.Service.Application.Commands.Conversations.GenerateCompletion;

public sealed record AssistantInput(
    string Model,
    string SystemPrompt,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null
);