namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

/// <summary>
/// Redraws <paramref name="Image"/> from <paramref name="Prompt"/> (img2img). The output keeps the
/// image's aspect ratio; <paramref name="Strength"/> (0-1) is how far it may move away from it.
/// </summary>
public sealed record EditImageRequest(
    string Prompt,
    byte[] Image,
    float Strength,
    string? Model = null,
    IReadOnlyList<ImageLoraRequest>? Loras = null
);
