using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant.Schemas;

public sealed record GetAssistantResponse(
    Id<Assistant> Id,
    string Model,
    string SystemPrompt,
    string AssistantName,
    string UserName,
    float? Temperature,
    int? MaxTokens,
    float? RepeatPenalty
);
