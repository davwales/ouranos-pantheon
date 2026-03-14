using Ouranos.Pantheon.Hermes.Service.Application.Commands.Conversations.GenerateCompletion;

namespace Ouranos.Pantheon.Hermes.Service.Application.Interfaces.Conversations;

public interface IGenerateChatCompletion
{
    IAsyncEnumerable<string> GenerateCompletionStream(
        ConversationInput conversation,
        CancellationToken cancellationToken = default
    );
}