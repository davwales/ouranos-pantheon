using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;

public sealed record ToolCallMessagePartDto(
    string CallId,
    string Name,
    string Arguments,
    AssistantToolStatus Status,
    string Output,
    bool IsTruncated,
    long? DurationMs = null
) : MessagePartDto;
