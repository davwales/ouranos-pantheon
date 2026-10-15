namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

public sealed record FunctionCallOutputInputItemDto(string CallId, string Output)
    : ResponseInputItemDto;
