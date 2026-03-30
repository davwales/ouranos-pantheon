using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;

public interface IOuranosMachineLearningClient
{
    IAsyncEnumerable<string> StreamChatCompletionAsync(
        string model,
        List<MessageDto> messages,
        float? temperature = null,
        int? maxTokens = null,
        float? frequencyPenalty = null,
        CancellationToken cancellationToken = default
    );

    Task<string> GenerateChatCompletionAsync(
        string model,
        List<MessageDto> messages,
        float? temperature = null,
        int? maxTokens = null,
        float? frequencyPenalty = null,
        CancellationToken cancellationToken = default
    );

    Task<List<List<ForecastPoint>>> GetPlutusForecasts(
        GetPlutusForecastsRequest payload,
        CancellationToken cancellationToken = default
    );
}
