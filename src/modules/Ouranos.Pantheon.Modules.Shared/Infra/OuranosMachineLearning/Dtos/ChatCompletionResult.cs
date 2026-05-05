namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

public sealed record ChatCompletionResult(
    string Content,
    ChatCompletionUsage? Usage
);