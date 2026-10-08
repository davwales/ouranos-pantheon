namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

public sealed record AssistantPrompt(
    string ModelIdentifier,
    string SystemPrompt,
    float? Temperature = null,
    int? MaxTokens = null
);
