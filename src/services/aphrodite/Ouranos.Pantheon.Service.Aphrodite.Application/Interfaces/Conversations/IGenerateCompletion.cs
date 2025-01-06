using Ouranos.Pantheon.Service.Aphrodite.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Aphrodite.Application.Interfaces.Conversations;

public interface IGenerateCompletion
{
    IAsyncEnumerable<string> GenerateCompletionStream(
        Conversation conversation,
        CancellationToken cancellationToken = default
    );
}