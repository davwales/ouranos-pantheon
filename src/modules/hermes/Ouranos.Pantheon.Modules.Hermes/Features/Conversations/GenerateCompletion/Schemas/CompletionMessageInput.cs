using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record CompletionMessageInput(
    string Content,
    Role Role,
    IReadOnlyList<CompletionToolCallInput>? ToolCalls = null,
    // The earlier images a user message refers to, such as one picked to edit.
    IReadOnlyList<ImageMessagePartDto>? Images = null
);
