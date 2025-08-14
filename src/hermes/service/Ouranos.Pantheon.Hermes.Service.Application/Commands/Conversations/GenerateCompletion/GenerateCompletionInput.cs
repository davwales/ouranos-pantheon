using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;

namespace Ouranos.Pantheon.Hermes.Service.Application.Commands.Conversations.GenerateCompletion;

public sealed record GenerateCompletionInput(
    ConversationInput Conversation
) : ICommand<StreamResponse<string, GenerateCompletionResponse>>;