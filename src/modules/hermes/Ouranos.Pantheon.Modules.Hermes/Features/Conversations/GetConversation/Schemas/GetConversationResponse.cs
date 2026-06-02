using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;

public sealed record GetConversationResponse(
    Id<Conversation> Id,
    string Name,
    bool IsPublic,
    Id<Folder>? FolderId,
    GetConversationPersonaResponse Persona,
    GetConversationModelResponse Model,
    List<GetConversationTraitResponse> Traits,
    List<GetConversationMessageResponse> Messages,
    GetConversationTokenUsageResponse? TokenUsage,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
