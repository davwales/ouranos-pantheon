using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record HermesChatContext(
    Id<Conversation>? ConversationId,
    IReadOnlyList<ImageLora> ImageLoras,
    ConversationImages Images
) : IConversationScope;
