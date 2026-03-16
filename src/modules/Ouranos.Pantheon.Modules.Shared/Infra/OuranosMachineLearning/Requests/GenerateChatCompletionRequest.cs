using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;

public sealed record GenerateChatCompletionRequest(
    string Model,
    List<MessageDto> Messages,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null
);