namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

public sealed record ResponseStreamChunk(
    string? Text,
    string? Reasoning,
    ChatCompletionUsage? Usage
);
