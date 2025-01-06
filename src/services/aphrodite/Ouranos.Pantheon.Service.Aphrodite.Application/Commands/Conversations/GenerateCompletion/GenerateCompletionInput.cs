using MediatR;
using Ouranos.Pantheon.Service.Aphrodite.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Aphrodite.Application.Commands.Conversations.GenerateCompletion;

public sealed record GenerateCompletionInput(
    Conversation Conversation
) : IStreamRequest<GenerateCompletionResponse>;