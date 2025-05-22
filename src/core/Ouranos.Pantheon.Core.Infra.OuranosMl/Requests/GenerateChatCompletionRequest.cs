using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;

public sealed record GenerateChatCompletionRequest(
    string Model,
    List<MessageDto> Messages,
    float? Temperature = null,
    int? MaxTokens = null,
    float? RepeatPenalty = null
);