using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;

public interface IOuranosMachineLearningClient
{
    IAsyncEnumerable<string> GenerateCompletion(
        GenerateCompletionRequest payload,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<string> GenerateChatCompletion(
        GenerateChatCompletionRequest payload,
        CancellationToken cancellationToken = default
    );

    Task<List<List<ForecastPoint>>> GetPlutusForecasts(
        GetPlutusForecastsRequest payload,
        CancellationToken cancellationToken = default
    );
}