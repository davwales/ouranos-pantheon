namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

/// <summary>
/// A LoRA to apply on top of the image model, by its OuranosMl id.
/// </summary>
public sealed record ImageLoraRequest(string Name, float Weight);
