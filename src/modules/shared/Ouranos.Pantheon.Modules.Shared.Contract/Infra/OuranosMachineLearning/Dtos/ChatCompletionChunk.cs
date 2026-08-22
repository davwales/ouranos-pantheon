namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

public sealed record ChatCompletionChunk(string? Text, ChatCompletionUsage? Usage);
