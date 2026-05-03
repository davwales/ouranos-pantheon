namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

public sealed record ChatCompletionChunk(
    string? Text,
    ChatCompletionUsage? Usage
);