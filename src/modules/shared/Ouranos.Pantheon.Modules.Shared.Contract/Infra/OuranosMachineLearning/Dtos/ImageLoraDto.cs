namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

/// <summary>
/// A LoRA on the image server. It only suits image models with the same <paramref name="CompatClass"/>.
/// </summary>
public sealed record ImageLoraDto(
    string Id,
    string? Title,
    string? CompatClass,
    float? DefaultWeight,
    string? TriggerPhrase,
    string? UsageHint
);
