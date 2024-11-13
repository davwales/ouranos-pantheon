using MediatR;
using Talos.Olympus.Service.Aphrodite.Domain.Conversations;

namespace Talos.Olympus.Service.Aphrodite.Application.Commands.Conversations.GenerateCompletion;

public sealed record GenerateCompletionInput(
    Conversation Conversation
) : IStreamRequest<GenerateCompletionResponse>;