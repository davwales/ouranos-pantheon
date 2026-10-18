using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;

public sealed record GetConversationResponse(
    Id<Conversation> Id,
    string Name,
    bool IsPublic,
    Id<Folder>? FolderId,
    ReasoningEffort? ReasoningEffort,
    Id<ToolSet>? ToolSetId,
    GetConversationPersonaResponse Persona,
    GetConversationModelResponse Model,
    List<GetConversationTraitResponse> Traits,
    List<GetConversationLoraResponse> Loras,
    List<GetConversationMessageResponse> Messages,
    GetConversationTokenUsageResponse? TokenUsage,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
