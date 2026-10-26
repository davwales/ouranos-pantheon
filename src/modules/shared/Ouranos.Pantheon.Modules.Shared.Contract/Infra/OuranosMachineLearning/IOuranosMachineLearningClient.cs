using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

public interface IOuranosMachineLearningClient
{
    IAsyncEnumerable<ResponseStreamChunk> StreamResponseAsync(
        string model,
        string instructions,
        IReadOnlyList<ResponseInputItemDto> input,
        IReadOnlyList<FunctionToolDto> tools,
        float? temperature = null,
        int? maxTokens = null,
        ReasoningEffort? reasoningEffort = null,
        CancellationToken cancellationToken = default
    );

    Task<GeneratedResponse> GenerateResponseAsync(
        string model,
        string instructions,
        IReadOnlyList<ResponseInputItemDto> input,
        float? temperature = null,
        int? maxTokens = null,
        ReasoningEffort? reasoningEffort = null,
        CancellationToken cancellationToken = default
    );

    Task<T?> GenerateStructuredResponseAsync<T>(
        string model,
        string instructions,
        List<MessageDto> input,
        float? temperature = null,
        int? maxTokens = null,
        ReasoningEffort? reasoningEffort = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    Task<List<List<ForecastPoint>>> GetPlutusForecasts(
        GetPlutusForecastsRequest payload,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<ImageGenerationEventDto> StreamImageGenerationAsync(
        GenerateImageRequest payload,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<ImageGenerationEventDto> StreamImageEditAsync(
        EditImageRequest payload,
        CancellationToken cancellationToken = default
    );

    Task<List<AvailableModelDto>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default
    );
}
