using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl;

public interface IOuranosMachineLearningClient
{
    IAsyncEnumerable<string> GenerateCompletion(
        GenerateCompletionRequest payload,
        CancellationToken cancellationToken = default
    );
}