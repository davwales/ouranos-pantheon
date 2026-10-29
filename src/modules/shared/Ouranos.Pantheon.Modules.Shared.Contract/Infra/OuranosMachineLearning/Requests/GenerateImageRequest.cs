namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

public sealed record GenerateImageRequest(
    string Prompt,
    int Width,
    int Height,
    string? Model = null,
    IReadOnlyList<ImageLoraRequest>? Loras = null
);
