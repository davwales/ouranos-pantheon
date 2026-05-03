using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;

public sealed record GetConversationModelResponse(
    Id<ModelConfig> Id,
    string Name,
    string ModelIdentifier,
    string SystemPrompt,
    float? Temperature,
    int? MaxTokens,
    float? RepeatPenalty,
    int? ContextWindow
);
