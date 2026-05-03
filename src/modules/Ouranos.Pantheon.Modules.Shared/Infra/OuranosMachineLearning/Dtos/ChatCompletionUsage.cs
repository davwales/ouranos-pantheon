namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

public sealed record ChatCompletionUsage(
    int InputTokens,
    int OutputTokens,
    int TotalTokens
);