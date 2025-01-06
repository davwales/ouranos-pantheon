using Ouranos.Pantheon.Service.Hermes.Domain.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;

public interface IGenerateCompletion
{
    IAsyncEnumerable<string> GenerateCompletionStream(
        Conversation conversation,
        CancellationToken cancellationToken = default
    );
}