using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record GenerateCompletionInput(
    ConversationInput Conversation
) : ICommand<StreamResponse<string, GenerateCompletionResponse>>;
