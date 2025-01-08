using Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Application.Queries.Conversations.GetCompletion;

public sealed record ConversationInput(
    CharacterInput User,
    CharacterInput Assistant,
    string Context,
    List<Message> Messages
);