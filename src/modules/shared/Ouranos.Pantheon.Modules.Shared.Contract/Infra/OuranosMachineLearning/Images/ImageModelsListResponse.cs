using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Images;

internal sealed record ImageModelsListResponse(string DefaultModel, List<ImageModelDto> Data);
