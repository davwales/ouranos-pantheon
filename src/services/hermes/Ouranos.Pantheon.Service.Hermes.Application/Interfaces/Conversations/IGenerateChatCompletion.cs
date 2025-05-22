using Ouranos.Pantheon.Service.Hermes.Application.Commands.Conversations.GenerateCompletion;

namespace Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;

public interface IGenerateChatCompletion
{
    IAsyncEnumerable<string> GenerateCompletionStream(
        ConversationInput conversation,
        CancellationToken cancellationToken = default
    );
}