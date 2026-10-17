namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

public sealed record ImageGenerationCompletedDto(
    byte[] Content,
    string MediaType,
    int Width,
    int Height,
    long? Seed
) : ImageGenerationEventDto;
