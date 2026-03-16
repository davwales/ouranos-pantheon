using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Requests;

public sealed record GenerateCompletionRequest(
    List<MessageDto> Messages
);