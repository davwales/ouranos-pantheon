using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;
using Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl;

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