namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

public sealed record ChatCompletionResult(string Content, ChatCompletionUsage? Usage);
