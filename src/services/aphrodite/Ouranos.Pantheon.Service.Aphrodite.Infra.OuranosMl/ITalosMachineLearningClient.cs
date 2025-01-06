using Ouranos.Pantheon.Service.Aphrodite.Infra.OuranosMl.Requests;

namespace Ouranos.Pantheon.Service.Aphrodite.Infra.OuranosMl;

public interface IOuranosMachineLearningClient
{
    IAsyncEnumerable<string> GenerateCompletion(
        GenerateCompletionRequest payload,
        CancellationToken cancellationToken = default
    );
}