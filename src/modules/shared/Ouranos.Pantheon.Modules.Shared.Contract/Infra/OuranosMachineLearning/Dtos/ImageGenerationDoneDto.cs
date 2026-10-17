namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

public sealed record ImageGenerationDoneDto(string Model) : ImageGenerationEventDto;
