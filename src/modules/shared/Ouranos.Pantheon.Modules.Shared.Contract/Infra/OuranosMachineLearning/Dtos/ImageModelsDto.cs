namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

/// <summary>
/// The image models OuranosMl can run, and the one it uses when a request names none.
/// </summary>
public sealed record ImageModelsDto(string DefaultModel, IReadOnlyList<ImageModelDto> Models);
