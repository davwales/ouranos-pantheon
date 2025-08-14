using Ouranos.Pantheon.Hermes.Service.Domain.Conversations;

namespace Ouranos.Pantheon.Hermes.Service.Application.Commands.Conversations.GenerateCompletion;

public sealed record ConversationInput(
    AssistantInput Assistant,
    List<Message> Messages
);