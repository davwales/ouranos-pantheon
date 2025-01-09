using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;

public sealed record GenerateCompletionRequest(
    List<MessageDto> Messages
);