using MediatR;

namespace Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;

public sealed record GenerateCompletionInput(
    ConversationInput Conversation
) : IStreamRequest<GenerateCompletionResponse>;