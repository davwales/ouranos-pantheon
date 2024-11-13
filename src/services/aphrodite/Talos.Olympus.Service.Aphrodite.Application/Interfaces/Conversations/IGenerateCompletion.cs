using Talos.Olympus.Service.Aphrodite.Domain.Conversations;

namespace Talos.Olympus.Service.Aphrodite.Application.Interfaces.Conversations;

public interface IGenerateCompletion
{
    IAsyncEnumerable<string> GenerateCompletionStream(
        Conversation conversation,
        CancellationToken cancellationToken = default
    );
}