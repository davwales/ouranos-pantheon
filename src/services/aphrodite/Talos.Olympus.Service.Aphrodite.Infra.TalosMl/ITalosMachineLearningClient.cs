using Talos.Olympus.Service.Aphrodite.Infra.TalosMl.Requests;

namespace Talos.Olympus.Service.Aphrodite.Infra.TalosMl;

public interface ITalosMachineLearningClient
{
    IAsyncEnumerable<string> GenerateCompletion(
        GenerateCompletionRequest payload,
        CancellationToken cancellationToken = default
    );
}