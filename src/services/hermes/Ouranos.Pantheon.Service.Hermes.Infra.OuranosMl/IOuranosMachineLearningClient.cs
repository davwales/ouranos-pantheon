using Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl.Requests;

namespace Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl;

public interface IOuranosMachineLearningClient
{
    IAsyncEnumerable<string> GenerateCompletion(
        GenerateCompletionRequest payload,
        CancellationToken cancellationToken = default
    );
}