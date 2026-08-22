using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

public interface IOuranosMachineLearningClient
{
    IAsyncEnumerable<ChatCompletionChunk> StreamChatCompletionAsync(
        string model,
        List<MessageDto> messages,
        float? temperature = null,
        int? maxTokens = null,
        float? frequencyPenalty = null,
        CancellationToken cancellationToken = default
    );

    Task<ChatCompletionResult> GenerateChatCompletionAsync(
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

    Task<List<AvailableModelDto>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default
    );
}
