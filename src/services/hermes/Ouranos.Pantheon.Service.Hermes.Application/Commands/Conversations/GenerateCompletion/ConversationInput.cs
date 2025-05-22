using Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;

public sealed record ConversationInput(
    AssistantInput Assistant,
    List<Message> Messages
);