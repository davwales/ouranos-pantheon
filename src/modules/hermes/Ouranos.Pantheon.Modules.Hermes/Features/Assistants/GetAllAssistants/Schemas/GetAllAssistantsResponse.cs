using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants.Schemas;

public sealed record GetAllAssistantsResponse(
    Id<Assistant> Id,
    string Model,
    string SystemPrompt,
    string AssistantName,
    string UserName,
    float? Temperature,
    int? MaxTokens,
    float? RepeatPenalty
);
